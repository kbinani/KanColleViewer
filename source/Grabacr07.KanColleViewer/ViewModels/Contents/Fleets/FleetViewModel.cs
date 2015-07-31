using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Grabacr07.KanColleViewer.Properties;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Livet;
using Livet.EventListeners;

namespace Grabacr07.KanColleViewer.ViewModels.Contents.Fleets
{
	/// <summary>
	/// 単一の艦隊情報を提供します。
	/// </summary>
	public class FleetViewModel : ItemViewModel
	{
		public Fleet Source { get; }

		public int Id => this.Source.Id;

		public string Name => string.IsNullOrEmpty(this.Source.Name.Trim()) ? "(第 " + this.Source.Id + " 艦隊)" : this.Source.Name;

		/// <summary>
		/// 艦隊に所属している艦娘のコレクションを取得します。
		/// </summary>
		public ShipViewModel[] Ships
		{
			get { return this.Source.Ships.Select(x => new ShipViewModel(x)).ToArray(); }
		}

		public FleetStateViewModel State { get; }

		public ExpeditionViewModel Expedition { get; }

		public ViewModel QuickStateView
		{
			get
			{
				var situation = this.Source.State.Situation;
				if (situation == FleetSituation.Empty)
				{
					return NullViewModel.Instance;
				}
				if (situation.HasFlag(FleetSituation.Sortie))
				{
					return this.State.Sortie;
				}
				if (situation.HasFlag(FleetSituation.Expedition))
				{
					return this.Expedition;
				}

				return this.State.Homeport;
			}
		}


		public FleetViewModel(Fleet fleet)
		{
			this.Source = fleet;

			this.CompositeDisposable.Add(new PropertyChangedEventListener(fleet)
			{
				(sender, args) => this.RaisePropertyChanged(args.PropertyName),
			});
			this.CompositeDisposable.Add(new PropertyChangedEventListener(fleet.State)
			{
				{ nameof(fleet.State.Situation), (sender, args) => this.RaisePropertyChanged(nameof(this.QuickStateView)) },
			});

			this.State = new FleetStateViewModel(fleet.State);
			this.CompositeDisposable.Add(this.State);

			this.Expedition = new ExpeditionViewModel(fleet.Expedition);
			this.CompositeDisposable.Add(this.Expedition);
		}

        public void CopyFleetInformation()
        {
            var information = Ships.Select(s =>
            {
                var ship = s.Ship;
                var slotItemNames = ship.Slots
                    .Where(equip => equip.Item != null && equip.Item.Info != null && equip.Item.Info.Name != "？？？")
                    .Select(equip => equip.ShortName() + equip.Item.LevelText);
                var extSlotItemName = (ship.ExSlot != null && ship.ExSlot.Item != null && ship.ExSlot.Item.Info != null && ship.ExSlot.Item.Info.Name != "？？？") ? ship.ExSlot.ShortName() + ship.ExSlot.Item.LevelText : "";
                return ship.Info.Name + ship.Level + "{cond:" + ship.Condition + ",HP:" + ship.HP.Current + "/" + ship.HP.Maximum + "}[" + slotItemNames.ToString("/") + "]" + "[" + extSlotItemName + "]";
            }).ToString("\n:") + "\n";
            Clipboard.SetText(information);
        }
    }

    static class SlotitemHelper
    {
        public static string ShortName(this ShipSlot item)
        {
            var map = new Dictionary<string, string> {
                {"41cm連装砲", "41"},
                {"試製41cm三連装砲", "試41"},
                {"35.6cm連装砲", "35.6"},
                {"試製35.6cm三連装砲", "試35.6"},
                {"46cm三連装砲", "46"},
                {"試製51cm連装砲", "試51"},
                {"21号対空電探", "21号"},
                {"20.3cm連装砲", "20.3"},
                {"12.7cm連装砲", "12.7"},
                {"瑞雲", "瑞"},
                {"22号対水上電探", "22号"},
                {"13号対空電探改", "13号改"},
                {"零式水上偵察機", "零偵"},
                {"零式水上観測機", "零観"},
                {"甲標的", "甲"},
                {"九九式艦爆(江草隊)", "江草隊"},
                {"九七式艦攻(友永隊)", "友永隊"},
                {"九一式徹甲弾", "徹甲弾"},
            };
            var longName = item.Item.Info.Name;
            return map.ContainsKey(longName) ? map[longName] : longName;
        }
    }
}
