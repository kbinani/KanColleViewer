using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;
using Grabacr07.KanColleWrapper.Internal;

namespace Grabacr07.KanColleWrapper.Models
{
	/// <summary>
	/// 艦娘の種類に基づく情報を表します。
	/// </summary>
	public class ShipInfo : RawDataWrapper<kcsapi_mst_ship>, IIdentifiable
	{
		private ShipType shipType;

		/// <summary>
		/// 艦を一意に識別する ID を取得します。
		/// </summary>
		public int Id => this.RawData.api_id;

		public int SortId => this.RawData.api_sortno;

		/// <summary>
		/// 艦の名称を取得します。
		/// </summary>
		public string Name => this.RawData.api_name;

		/// <summary>
		/// 艦種を取得します。
		/// </summary>
		public ShipType ShipType => this.shipType ?? (this.shipType = KanColleClient.Current.Master.ShipTypes[this.RawData.api_stype]) ?? ShipType.Dummy;

		/// <summary>
		/// 各装備スロットの最大搭載機数を取得します。
		/// </summary>
		public int[] Slots => this.RawData.api_maxeq;

		#region 用意したけど使わないっぽい？

		/// <summary>
		/// よみがなを取得します。
		/// </summary>
		public string Kana => this.RawData.api_yomi;

		/// <summary>
		/// 火力の最大値を取得します。
		/// </summary>
		public int MaxFirepower => this.RawData.api_houg.Get(1) ?? 0;

		/// <summary>
		/// 装甲の最大値を取得します。
		/// </summary>
		public int MaxArmer => this.RawData.api_souk.Get(1) ?? 0;

		/// <summary>
		/// 雷装の最大値を取得します。
		/// </summary>
		public int MaxTorpedo => this.RawData.api_raig.Get(1) ?? 0;

		/// <summary>
		/// 対空の最大値を取得します。
		/// </summary>
		public int MaxAA => this.RawData.api_tyku.Get(1) ?? 0;

		/// <summary>
		/// 運の最小値を取得します。
		/// </summary>
		public int MinLuck => this.RawData.api_luck.Get(0) ?? 0;

		/// <summary>
		/// 運の最大値を取得します。
		/// </summary>
		public int MaxLuck => this.RawData.api_luck.Get(1) ?? 0;

		/// <summary>
		/// 耐久値を取得します。
		/// </summary>
		public int HP => this.RawData.api_taik.Get(0) ?? 0;

		/// <summary>
		/// 燃料の最大値を取得します。
		/// </summary>
		public int MaxFuel => this.RawData.api_fuel_max;

		/// <summary>
		/// 弾薬の最大値を取得します。
		/// </summary>
		public int MaxBull => this.RawData.api_bull_max;

		/// <summary>
		/// 装備可能スロット数を取得します。
		/// </summary>
		public int SlotCount => this.RawData.api_slot_num;

		#endregion

		/// <summary>
		/// 速力を取得します。
		/// </summary>
		public ShipSpeed Speed => (ShipSpeed)this.RawData.api_soku;

		/// <summary>
		/// 次の改造が実施できるレベルを取得します。
		/// </summary>
		public int? NextRemodelingLevel => this.RawData.api_afterlv == 0 ? null : (int?)this.RawData.api_afterlv;

		internal ShipInfo(kcsapi_mst_ship rawData) : base(rawData) { }

        public string ShipClass
        {
            get
            {
                string name = this.Name;
                var suffixList = new string[] { "改", "改二", " zwei", " drei" };
                foreach (var suffix in suffixList)
                {
                    if (name.EndsWith(suffix))
                    {
                        name = name.Substring(0, name.Length - suffix.Length);
                    }
                }
                switch (name)
                {
                    case "金剛":
                    case "比叡":
                    case "榛名":
                    case "霧島":
                        {
                            return "金剛型";
                        }
                    case "扶桑":
                    case "山城":
                        {
                            return "扶桑型";
                        }
                    case "伊勢":
                    case "日向":
                        {
                            return "伊勢型";
                        }
                    case "長門":
                    case "陸奥":
                        {
                            return "長門型";
                        }
                    case "大和":
                    case "武蔵":
                        {
                            return "大和型";
                        }
                    case "Bismarck":
                        {
                            return "ビスマルク級";
                        }
                    case "Littorio":
                    case "Italia":
                    case "Roma":
                        {
                            return "V・ヴェネト級";
                        }
                    case "翔鶴":
                    case "瑞鶴":
                        {
                            return "翔鶴型";
                        }
                    case "雲龍":
                    case "天城":
                    case "葛城":
                        {
                            return "雲龍型";
                        }
                    case "大鳳":
                        {
                            return "大鳳型";
                        }
                    case "祥鳳":
                    case "瑞鳳":
                        {
                            return "祥鳳型";
                        }
                    case "飛鷹":
                    case "隼鷹":
                        {
                            return "飛鷹型";
                        }
                    case "千歳":
                    case "千歳甲":
                    case "千歳航":
                    case "千代田":
                    case "千代田甲":
                    case "千代田航":
                        {
                            return "千歳型";
                        }
                    case "秋津洲":
                        {
                            return "飛行艇母艦";
                        }
                    case "古鷹":
                    case "加古":
                        {
                            return "古鷹型";
                        }
                    case "青葉":
                    case "衣笠":
                        {
                            return "青葉型";
                        }
                    case "妙高":
                    case "那智":
                    case "足柄":
                    case "羽黒":
                        {
                            return "妙高型";
                        }
                    case "高雄":
                    case "愛宕":
                    case "摩耶":
                    case "鳥海":
                        {
                            return "高雄型";
                        }
                    case "最上":
                    case "三隈":
                    case "鈴谷":
                    case "熊野":
                        {
                            return "最上型";
                        }
                    case "利根":
                    case "筑摩":
                        {
                            return "利根型";
                        }
                    case "Prinz Eugen":
                        {
                            return "A・ヒッパー級";
                        }
                    case "天龍":
                    case "龍田":
                        {
                            return "天龍型";
                        }
                    case "球磨":
                    case "多摩":
                    case "北上":
                    case "大井":
                    case "木曾":
                        {
                            return "球磨型";
                        }
                    case "長良":
                    case "五十鈴":
                    case "名取":
                    case "由良":
                    case "鬼怒":
                    case "阿武隈":
                        {
                            return "長良型";
                        }
                    case "川内":
                    case "神通":
                    case "那珂":
                        {
                            return "川内型";
                        }
                    case "夕張":
                        {
                            return "夕張型";
                        }
                    case "阿賀野":
                    case "能代":
                    case "矢矧":
                    case "酒匂":
                        {
                            return "阿賀野型";
                        }
                    case "大淀":
                        {
                            return "大淀型";
                        }
                    case "睦月":
                    case "如月":
                    case "弥生":
                    case "卯月":
                    case "皐月":
                    case "文月":
                    case "長月":
                    case "菊月":
                    case "三日月":
                    case "望月":
                        {
                            return "睦月型";
                        }
                    case "吹雪":
                    case "白雪":
                    case "初雪":
                    case "深雪":
                    case "叢雲":
                    case "磯波":
                        {
                            return "吹雪型";
                        }
                    case "綾波":
                    case "敷波":
                    case "朧":
                    case "曙":
                    case "漣":
                    case "潮":
                        {
                            return "綾波型";
                        }
                    case "暁":
                    case "電":
                    case "響":
                    case "Верный":
                    case "雷":
                        {
                            return "暁型";
                        }
                    case "初春":
                    case "子日":
                    case "若葉":
                    case "初霜":
                        {
                            return "初春型";
                        }
                    case "白露":
                    case "時雨":
                    case "村雨":
                    case "夕立":
                    case "春雨":
                    case "五月雨":
                    case "涼風":
                    case "江風":
                    case "海風":
                        {
                            return "白露型";
                        }
                    case "朝潮":
                    case "大潮":
                    case "満潮":
                    case "荒潮":
                    case "朝雲":
                    case "山雲":
                    case "霰":
                    case "霞":
                        {
                            return "朝潮型";
                        }
                    case "陽炎":
                    case "不知火":
                    case "黒潮":
                    case "初風":
                    case "雪風":
                    case "天津風":
                    case "時津風":
                    case "浦風":
                    case "磯風":
                    case "浜風":
                    case "谷風":
                    case "野分":
                    case "舞風":
                    case "秋雲":
                        {
                            return "陽炎型";
                        }
                    case "夕雲":
                    case "巻雲":
                    case "長波":
                    case "高波":
                    case "朝霜":
                    case "早霜":
                    case "清霜":
                    case "風雲":
                        {
                            return "夕雲型";
                        }
                    case "秋月":
                    case "照月":
                        {
                            return "秋月型";
                        }
                    case "島風":
                        {
                            return "島風型";
                        }
                    case "Z1":
                    case "Z3":
                        {
                            return "Z1型";
                        }
                    case "伊168":
                        {
                            return "海大VI型";
                        }
                    case "伊8":
                        {
                            return "巡潜３型";
                        }
                    case "伊19":
                        {
                            return "巡潜乙型";
                        }
                    case "伊58":
                        {
                            return "巡潜乙型改二";
                        }
                    case "U-511":
                        {
                            return "UボートIXC型";
                        }
                    case "呂500":
                        {
                            return "呂号潜水艦";
                        }
                    case "まるゆ":
                        {
                            return "三式潜航輸送艇";
                        }
                    case "伊401":
                        {
                            return "潜特型";
                        }
                    case "あきつ丸":
                        {
                            return "特種船丙型";
                        }
                    case "大鯨":
                        {
                            return "潜水母艦";
                        }
                    case "明石":
                        {
                            return "工作艦";
                        }
                    case "香取":
                        {
                            return "練習巡洋艦";
                        }

                    default:
                        {
                            return "(不明)";
                        }
                }
            }
        }

		public override string ToString()
		{
			return $"ID = {this.Id}, Name = \"{this.Name}\", ShipType = \"{this.ShipType.Name}\"";
		}

		#region static members

		public static ShipInfo Dummy { get; } = new ShipInfo(new kcsapi_mst_ship
		{
			api_id = 0,
			api_name = "？？？"
		});

		#endregion
	}
}
