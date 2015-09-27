﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.IO;
using Nekoxy;
using StatefulModel;

namespace Grabacr07.KanColleWrapper
{
    public partial class KanColleProxy
    {
        private readonly IConnectableObservable<Session> connectableSessionSource;
        private readonly IConnectableObservable<Session> apiSource;
        private readonly CompositeDisposable compositeDisposable;
        private SortedSet<string> staticContents;
        private string staticContentsLogFilePath;

        public IObservable<Session> SessionSource => this.connectableSessionSource.AsObservable();

        public IObservable<Session> ApiSessionSource => this.apiSource.AsObservable();

        #region UpstreamProxySettingsプロパティ

        private IProxySettings _UpstreamProxySettings;

        public IProxySettings UpstreamProxySettings
        {
            get { return this._UpstreamProxySettings; }
            set
            {
                this._UpstreamProxySettings = value;
                this.ApplyProxySettings();
            }
        }

        #endregion

        public int ListeningPort { get; private set; } = 37564;

        public KanColleProxy()
        {
            this.compositeDisposable = new CompositeDisposable();

            this.connectableSessionSource = Observable
                .FromEvent<Action<Session>, Session>(
                    action => action,
                    h => HttpProxy.AfterSessionComplete += h,
                    h => HttpProxy.AfterSessionComplete -= h)
                .Publish();

            var logFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var log = new StreamWriter(logFilePath + ".log", true);
            staticContentsLogFilePath = logFilePath + ".static_contents.log";
            staticContents = new SortedSet<string>();
            if (File.Exists(staticContentsLogFilePath))
            {
                foreach (var line in File.ReadLines(staticContentsLogFilePath))
                {
                    staticContents.Add(line);
                }
            }

            this.apiSource = this.connectableSessionSource
                .Do(s =>
                {
                    try
                    {
                        if (s.Request.PathAndQuery.StartsWith("/kcsapi") && s.Response.MimeType == "text/plain")
                        {
                            log.WriteLine("=========================");
                            log.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>");
                            log.WriteLine(s.Request.RequestLine);
                            log.WriteLine(s.Request.Headers.HeadersInOrder);
                            log.WriteLine();
                            log.WriteLine(s.Request.BodyAsString);
                            log.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<");
                            log.WriteLine(s.Response.Headers.HeadersInOrder);
                            log.WriteLine();
                            log.WriteLine(s.Response.BodyAsString);
                        }
                    }
                    catch (Exception e)
                    {

                    }

                    try
                    {
//                        if (s.hostname == s.m_hostIP)
//                        {
                            string url = s.Request.PathAndQuery;
                            if (url.Contains("?"))
                            {
                                url = url.Substring(0, url.IndexOf("?"));
                            }
                            staticContents.Add(url);
//                        }
                    }
                    catch (Exception e)
                    {

                    }
                })
            #region .Do(debug)
#if DEBUG
.Do(session =>
                {
                    Debug.WriteLine("==================================================");
                    Debug.WriteLine("Nekoxy session: ");
                    Debug.WriteLine(session);
                    Debug.WriteLine("");
                })
#endif
            #endregion
                .Publish();
        }


        public void Startup(int proxy = 37564)
        {
            this.ListeningPort = proxy;
            
            HttpProxy.Startup(proxy, false, false);
            this.ApplyProxySettings();

            this.compositeDisposable.Add(this.connectableSessionSource.Connect());
            this.compositeDisposable.Add(this.apiSource.Connect());
        }

        public void Shutdown()
        {
            this.compositeDisposable.Dispose();
            HttpProxy.Shutdown();

            using (var writer = new StreamWriter(staticContentsLogFilePath, false, System.Text.Encoding.UTF8))
            {
                foreach (var s in staticContents)
                {
                    writer.WriteLine(s);
                }
            }
        }

        /// <summary>
        /// プロキシ設定を反映
        /// </summary>
        private void ApplyProxySettings()
        {
            this.ApplyUpstreamProxySettings();
            this.ApplyDownstreamProxySettings();
        }

        /// <summary>
        /// 上流プロキシを設定
        /// </summary>
        private void ApplyUpstreamProxySettings()
        {
            switch (this.UpstreamProxySettings?.Type)
            {
                case ProxyType.DirectAccess:
                    HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.DirectAccess);
                    break;
                case ProxyType.SystemProxy:
                    HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.SystemProxy);
                    break;
                case ProxyType.SpecificProxy:
                    HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.SpecificProxy, this.UpstreamProxySettings.Host, this.UpstreamProxySettings.Port);
                    break;
                default:
                    //UpstreamProxySettings == null は SystemProxy使用とみなす
                    HttpProxy.UpstreamProxyConfig = new ProxyConfig(ProxyConfigType.SystemProxy);
                    break;
            }
        }

        /// <summary>
        /// HttpProxy.UpstreamProxyConfig を元に、下流からの通信がNekoxyを通るよう設定
        /// </summary>
        private void ApplyDownstreamProxySettings()
        {
            var config = HttpProxy.UpstreamProxyConfig;
            switch (config.Type)
            {
                case ProxyConfigType.SystemProxy:
                    WinInetUtil.SetProxyInProcessForNekoxy(this.ListeningPort);
                    break;
                case ProxyConfigType.SpecificProxy:
                    //プロキシを使用しない場合、HTTPだけNekoxyを通し、後は直アクセス
                    if (!string.IsNullOrWhiteSpace(config.SpecificProxyHost))
                    {
                        WinInetUtil.SetProxyInProcess(
                            $"http=127.0.0.1:{this.ListeningPort};"
                            + $"https={config.SpecificProxyHost}:{config.SpecificProxyPort};"
                            + $"ftp={config.SpecificProxyHost}:{config.SpecificProxyPort};",
                            "local");
                    }
                    else
                    {
                        //UpstreamProxyHost が空の場合は直アクセスとみなす
                        WinInetUtil.SetProxyInProcess($"http=127.0.0.1:{this.ListeningPort}", "local");
                    }
                    break;
                case ProxyConfigType.DirectAccess:
                    //指定プロキシの場合、HTTPだけNekoxyを通し、後は指定プロキシに流す
                    WinInetUtil.SetProxyInProcess($"http=127.0.0.1:{this.ListeningPort}", "local");
                    break;
                default:
                    break;
            }
        }
    }
}
