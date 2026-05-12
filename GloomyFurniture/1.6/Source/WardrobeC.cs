using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Gloomylynx
{
    /// <summary>
    /// 책장과 동일한 보관(ThingOwner) 패턴. IApparelSource + 기본 true로 의류 정책 자동 착용 허용, 지즈모로 끄면 저장 전용.
    /// </summary>
    public class Building_GloomyWardrobeC : Building, IThingHolderEvents<Apparel>, IHaulEnroute, ILoadReferenceable,
        IStorageGroupMember, IHaulDestination, IStoreSettingsParent, IHaulSource, IThingHolder, IApparelSource,
        ISearchableContents, IBeautyContainer
    {
        private ThingOwner<Apparel> innerContainer;

        private StorageSettings settings;

        private StorageGroup storageGroup;

        /// <summary>
        /// true면 의류 정책(JobGiver_OptimizeApparel)이 여기서 꺼내 입을 수 있음. false면 저장 전용(이전 무한 Wear 이슈 회피).
        /// </summary>
        private bool allowAutoWearFromWardrobe = true;

        private float cachedBeauty = -1f;

        public int MaximumApparel => this.def.building.maxItemsInCell * this.def.size.Area;

        public IReadOnlyList<Apparel> HeldApparel => this.innerContainer.InnerListForReading;

        public ThingOwner SearchableContents => this.innerContainer;

        public bool StorageTabVisible => true;

        public bool HaulSourceEnabled => true;

        public bool HaulDestinationEnabled => true;

        bool IApparelSource.ApparelSourceEnabled => this.allowAutoWearFromWardrobe;

        public bool RemoveApparel(Apparel apparel)
        {
            return this.innerContainer.Remove(apparel);
        }

        public Building_GloomyWardrobeC()
        {
            this.innerContainer = new ThingOwner<Apparel>(this, false, LookMode.Deep, true);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public StorageSettings GetStoreSettings()
        {
            if (this.storageGroup != null)
            {
                return this.storageGroup.GetStoreSettings();
            }
            return this.settings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return this.def.building.fixedStorageSettings;
        }

        public void Notify_SettingsChanged()
        {
            if (!base.Spawned)
            {
                return;
            }
            base.MapHeld.listerHaulables.Notify_HaulSourceChanged(this);
        }

        public bool Accepts(Thing t)
        {
            if (!(t is Apparel))
            {
                return false;
            }
            if (this.HeldApparel.Count >= this.MaximumApparel)
            {
                Apparel ap = t as Apparel;
                if (ap == null || !this.innerContainer.InnerListForReading.Contains(ap))
                {
                    return false;
                }
            }
            return this.GetStoreSettings().AllowedToAccept(t) && this.innerContainer.CanAcceptAnyOf(t, true);
        }

        public int SpaceRemainingFor(ThingDef def)
        {
            if (def == null || !def.IsApparel)
            {
                return 0;
            }
            return Mathf.Max(0, this.MaximumApparel - this.HeldApparel.Count);
        }

        StorageGroup IStorageGroupMember.Group
        {
            get => this.storageGroup;
            set => this.storageGroup = value;
        }

        bool IStorageGroupMember.DrawConnectionOverlay => base.Spawned;

        Map IStorageGroupMember.Map => base.MapHeld;

        string IStorageGroupMember.StorageGroupTag => this.def.building.storageGroupTag;

        StorageSettings IStorageGroupMember.StoreSettings => this.GetStoreSettings();

        StorageSettings IStorageGroupMember.ParentStoreSettings => this.GetParentStoreSettings();

        StorageSettings IStorageGroupMember.ThingStoreSettings => this.settings;

        bool IStorageGroupMember.DrawStorageTab => true;

        bool IStorageGroupMember.ShowRenameButton => base.Faction == Faction.OfPlayer;

        public float BeautyOffset
        {
            get
            {
                if (this.cachedBeauty < 0f)
                {
                    bool outdoors = this.GetRoom(RegionType.Set_All).PsychologicallyOutdoors;
                    this.cachedBeauty = this.HeldApparel.Aggregate(0f, (b, a) => b + a.GetBeauty(outdoors));
                }
                return this.cachedBeauty - 0.001f;
            }
        }

        string IBeautyContainer.BeautyOffsetExplanation =>
            string.Format("{0}: {1}", "ContainedApparelBeauty".Translate(), this.BeautyOffset.ToStringWithSign("0.##"));

        public void Notify_ItemAdded(Apparel item)
        {
            this.cachedBeauty = -1f;
            this.DirtyRoomStats();
            if (base.Spawned)
            {
                base.MapHeld.listerHaulables.Notify_AddedThing(item);
            }
        }

        public void Notify_ItemRemoved(Apparel item)
        {
            this.cachedBeauty = -1f;
            this.DirtyRoomStats();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (this.storageGroup != null && map != this.storageGroup.Map)
            {
                StorageSettings storeSettings = this.storageGroup.GetStoreSettings();
                this.storageGroup.RemoveMember(this, true);
                this.storageGroup = null;
                this.settings.CopyFrom(storeSettings);
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            this.settings = new StorageSettings(this);
            if (this.def.building.defaultStorageSettings != null)
            {
                this.settings.CopyFrom(this.def.building.defaultStorageSettings);
            }
            this.DirtyRoomStats();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.storageGroup != null)
            {
                this.storageGroup.RemoveMember(this, true);
                this.storageGroup = null;
            }
            if (mode != DestroyMode.WillReplace)
            {
                this.innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near, null, null, true);
            }
            base.DeSpawn(mode);
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (Find.Selector.SingleSelectedThing == this)
            {
                Room room = this.GetRoom(RegionType.Set_All);
                if (room != null && room.ProperRoom)
                {
                    room.DrawFieldEdges();
                }
            }
            StorageGroupUtility.DrawSelectionOverlaysFor(this);
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (!base.Spawned)
            {
                return text;
            }
            if (!string.IsNullOrEmpty(text))
            {
                text += "\n";
            }
            if (this.storageGroup != null)
            {
                text += string.Format("{0}: {1} ", "StorageGroupLabel".Translate(), this.storageGroup.RenamableLabel.CapitalizeFirst());
                if (this.storageGroup.MemberCount > 1)
                {
                    text += "(" + "NumBuildings".Translate(this.storageGroup.MemberCount).CapitalizeFirst() + ")\n";
                }
                else
                {
                    text += "(" + "OneBuilding".Translate() + ")\n";
                }
            }
            text += "GL_WardrobeCStoredInspect".Translate(this.HeldApparel.Count, this.MaximumApparel);
            text += "\n" + (this.allowAutoWearFromWardrobe
                ? "GL_WardrobeC_AutoWearOnInspect".Translate()
                : "GL_WardrobeC_AutoWearOffInspect".Translate());
            return text;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            foreach (Gizmo gizmo2 in StorageSettingsClipboard.CopyPasteGizmosFor(this.GetStoreSettings()))
            {
                yield return gizmo2;
            }
            if (this.StorageTabVisible && base.MapHeld != null)
            {
                foreach (Gizmo gizmo3 in StorageGroupUtility.StorageGroupMemberGizmos(this))
                {
                    yield return gizmo3;
                }
            }
            if (base.Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "GL_WardrobeC_AutoWearLabel".Translate(),
                    defaultDesc = "GL_WardrobeC_AutoWearDesc".Translate(),
                    icon = this.allowAutoWearFromWardrobe ? TexCommand.ForbidOff : TexCommand.ForbidOn,
                    isActive = () => this.allowAutoWearFromWardrobe,
                    toggleAction = delegate
                    {
                        this.allowAutoWearFromWardrobe = !this.allowAutoWearFromWardrobe;
                        if (base.Spawned && base.MapHeld != null)
                        {
                            base.MapHeld.listerHaulables.RecalculateAllInHaulSource(this);
                        }
                    }
                };
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption floatMenuOption in HaulSourceUtility.GetFloatMenuOptions(this, selPawn))
            {
                yield return floatMenuOption;
            }
            foreach (Apparel ap in this.HeldApparel)
            {
                foreach (FloatMenuOption floatMenuOption2 in ap.GetFloatMenuOptions(selPawn))
                {
                    yield return floatMenuOption2;
                }
            }
            foreach (FloatMenuOption floatMenuOption3 in base.GetFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption3;
            }
        }

        private void DirtyRoomStats()
        {
            Room room = this.GetRoom(RegionType.Set_All);
            room?.Notify_ContainedThingSpawnedOrDespawned(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref this.innerContainer, "innerContainer", new object[] { this });
            Scribe_Deep.Look(ref this.settings, "settings", new object[] { this });
            Scribe_References.Look(ref this.storageGroup, "storageGroup", false);
            Scribe_Values.Look(ref this.allowAutoWearFromWardrobe, "allowAutoWearFromWardrobe", true);
        }
    }

    public class ITab_ContentsGloomyWardrobeC : ITab_ContentsBase
    {
        private static readonly CachedTexture DropTex = new CachedTexture("UI/Buttons/Drop");

        public override IList<Thing> container =>
            this.Wardrobe.GetDirectlyHeldThings().ToList<Thing>();

        public override bool IsVisible => base.SelThing != null && base.IsVisible;

        public Building_GloomyWardrobeC Wardrobe => base.SelThing as Building_GloomyWardrobeC;

        public override bool VisibleInBlueprintMode => false;

        public ITab_ContentsGloomyWardrobeC()
        {
            this.labelKey = "TabCasketContents";
            this.containedItemsKey = "TabCasketContents";
        }

        protected override void DoItemsLists(Rect inRect, ref float curY)
        {
            this.ListApparel(inRect, this.container, ref curY);
        }

        private void ListApparel(Rect inRect, IList<Thing> things, ref float curY)
        {
            GUI.BeginGroup(inRect);
            Widgets.ListSeparator(ref curY, inRect.width, this.containedItemsKey.Translate());
            bool any = false;
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Apparel ap)
                {
                    any = true;
                    this.DoRow(ap, inRect.width, i, ref curY);
                }
            }
            if (!any)
            {
                Widgets.NoneLabel(ref curY, inRect.width, null);
            }
            GUI.EndGroup();
        }

        private void DoRow(Apparel apparel, float width, int i, ref float curY)
        {
            Rect rect = new Rect(0f, curY, width, 28f);
            Widgets.InfoCardButton(0f, curY, apparel);
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlightSelected(rect);
            }
            else if (i % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }
            Rect rect2 = new Rect(rect.width - 24f, curY, 24f, 24f);
            if (Widgets.ButtonImage(rect2, DropTex.Texture, true, null))
            {
                IntVec3 position;
                if (!(from x in this.Wardrobe.OccupiedRect().AdjacentCells
                        where x.Walkable(this.Wardrobe.Map)
                        select x).TryRandomElement(out position))
                {
                    position = this.Wardrobe.Position;
                }
                this.Wardrobe.GetDirectlyHeldThings().TryDrop(apparel, position, this.Wardrobe.Map, ThingPlaceMode.Near, 1,
                    out Thing dropped, null, null);
                if (dropped != null && dropped.TryGetComp(out CompForbiddable compForbiddable))
                {
                    compForbiddable.Forbidden = true;
                }
            }
            else if (Widgets.ButtonInvisible(rect, true))
            {
                Find.Selector.ClearSelection();
                Find.Selector.Select(apparel, true, true);
            }
            TooltipHandler.TipRegionByKey(rect2, "EjectApparelTooltip");
            Widgets.ThingIcon(new Rect(24f, curY, 28f, 28f), apparel, 1f, null, false, 1f, false);
            Rect rect3 = new Rect(60f, curY, rect.width - 36f, rect.height);
            rect3.xMax = rect2.xMin;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect3, apparel.LabelCap.Truncate(rect3.width, null));
            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(rect))
            {
                TargetHighlighter.Highlight(apparel, true, false, false);
                TooltipHandler.TipRegion(rect, apparel.DescriptionDetailed);
            }
            curY += 28f;
        }
    }
}
