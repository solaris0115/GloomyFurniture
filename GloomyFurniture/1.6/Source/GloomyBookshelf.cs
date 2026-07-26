using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Gloomylynx
{
    /// <summary>
    /// 글루미 책장: 남향일 때만 2열(1층 5권·2층 5권) 등 이미지.
    /// 기본은 drawLoc + bookOriginOffset(1권) + bookAlongStep×슬롯. matchVanillaBookcaseMetrics=true면 바닐라 책장 수식 + bookOriginOffset 추가.
    /// </summary>
    public class Building_GloomyBookshelf : Building_Bookcase
    {
        private static readonly Vector3 VanillaBookDrawOffset = new Vector3(0f, 0.018292684f, 0f);

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (this.def.drawerType == DrawerType.RealtimeOnly || !this.Spawned)
            {
                this.Graphic.Draw(drawLoc, flip ? this.Rotation.Opposite : this.Rotation, this, 0f);
            }

            SilhouetteUtility.DrawGraphicSilhouette(this, drawLoc);

            Comp_GloomyBookshelfVisual comp = this.TryGetComp<Comp_GloomyBookshelfVisual>();
            if (comp == null || this.Rotation != Rot4.South)
            {
                return;
            }

            CompProperties_GloomyBookshelfVisual p = comp.Props;
            Graphic spine = comp.SpineGraphic;
            if (spine == null)
            {
                return;
            }

            Rot4 bookRot = BookRotationForDraw();

            int count = this.HeldBooks.Count;
            for (int i = 0; i < count; i++)
            {
                int slotInRow = i % p.booksPerRow;
                bool secondRow = i >= p.booksPerRow;
                Vector3 loc = SlotWorldPosition(drawLoc, p, slotInRow, secondRow);
                spine.Draw(loc, bookRot, this, 0f);
            }
        }

        private Vector3 SlotWorldPosition(Vector3 drawLoc, CompProperties_GloomyBookshelfVisual p, int slotInRow, bool secondRow)
        {
            Vector3 rowShift = secondRow ? p.secondFloorOffset : Vector3.zero;

            if (p.matchVanillaBookcaseMetrics)
            {
                Rot4 rot = this.Rotation.Rotated(RotationDirection.Counterclockwise);
                float num = p.EffectiveSpacingAlong();
                Vector3 a = rot.FacingCell.ToVector3() * num;
                Vector3 bRow = rot.FacingCell.ToVector3() * (-(float)p.booksPerRow * num * 0.5f);
                Vector3 b2 = this.RotOffsets[this.Rotation.AsInt];
                return drawLoc + bRow + b2 + VanillaBookDrawOffset + (a * slotInRow) + rowShift + p.bookOriginOffset;
            }

            return drawLoc + p.bookOriginOffset + (p.bookAlongStep * slotInRow) + rowShift;
        }

        private Rot4 BookRotationForDraw()
        {
            Rot4 opposite = this.Rotation.Opposite;
            if (opposite == Rot4.East || opposite == Rot4.West)
            {
                opposite = opposite.Opposite;
            }

            return opposite;
        }

        public override void Notify_ColorChanged()
        {
            this.TryGetComp<Comp_GloomyBookshelfVisual>()?.InvalidateSpineGraphic();
            base.Notify_ColorChanged();
        }
    }

    public class Comp_GloomyBookshelfVisual : ThingComp
    {
        private Graphic spineGraphicInt;

        private int lastFirstHeldBookId = int.MinValue;

        private Vector2 lastDrawSizeUsed = Vector2.zero;

        public CompProperties_GloomyBookshelfVisual Props => (CompProperties_GloomyBookshelfVisual)this.props;

        public Graphic SpineGraphic
        {
            get
            {
                Building_GloomyBookshelf shelf = this.parent as Building_GloomyBookshelf;
                if (shelf == null || string.IsNullOrEmpty(this.Props.bookSpineTexPath))
                {
                    return null;
                }

                int firstId = shelf.HeldBooks.Count > 0 ? shelf.HeldBooks[0].thingIDNumber : 0;
                Vector2 drawSize = this.ResolveSpineDrawSize(shelf);

                if (this.spineGraphicInt == null || firstId != this.lastFirstHeldBookId ||
                    (this.lastDrawSizeUsed - drawSize).sqrMagnitude > 1E-06f)
                {
                    Shader shader = this.Props.useCutoutComplexShader ? ShaderDatabase.CutoutComplex : ShaderDatabase.Cutout;
                    this.spineGraphicInt = GraphicDatabase.Get<Graphic_Single>(
                        this.Props.bookSpineTexPath,
                        shader,
                        drawSize,
                        this.parent.DrawColor);
                    this.lastFirstHeldBookId = firstId;
                    this.lastDrawSizeUsed = drawSize;
                }

                return this.spineGraphicInt;
            }
        }

        private Vector2 ResolveSpineDrawSize(Building_GloomyBookshelf shelf)
        {
            Vector2 baseSize;

            if (shelf.HeldBooks.Count > 0)
            {
                baseSize = shelf.HeldBooks[0].VerticalGraphic.drawSize;
            }
            else
            {
                CompProperties_Book bookProps = ThingDefOf.TextBook.GetCompProperties<CompProperties_Book>();
                GraphicData vg = bookProps?.verticalGraphic;
                if (vg != null && vg.drawSize.sqrMagnitude > 0.0001f)
                {
                    baseSize = vg.drawSize;
                }
                else
                {
                    baseSize = ThingDefOf.TextBook.graphicData.drawSize;
                }
            }

            if (shelf.HeldBooks.Count == 0 &&
                (baseSize.sqrMagnitude < 0.0001f ||
                 (Mathf.Abs(baseSize.x - 1f) < 0.001f && Mathf.Abs(baseSize.y - 1f) < 0.001f)))
            {
                baseSize = this.Props.bookSpineBaseDrawSize;
            }

            return baseSize * this.Props.bookScale;
        }

        public void InvalidateSpineGraphic()
        {
            this.spineGraphicInt = null;
            this.lastFirstHeldBookId = int.MinValue;
            this.lastDrawSizeUsed = Vector2.zero;
        }
    }

    public class CompProperties_GloomyBookshelfVisual : CompProperties
    {
        public CompProperties_GloomyBookshelfVisual()
        {
            this.compClass = typeof(Comp_GloomyBookshelfVisual);
        }

        /// <summary>true면 바닐라 책장 bRow·축·RotOffsets·DrawOffset; bookAlongStep·bookOriginOffset(일반 모드의 “첫 권”)은 무시.</summary>
        public bool matchVanillaBookcaseMetrics = false;

        /// <summary>
        /// 인접 슬롯 간 거리(월드). matchVanillaBookcaseMetrics 경로. 음수면 0.155 고정(이 책장은 남향에서만 책 표시). 0 이상이면 그 값.
        /// </summary>
        public float bookSpacingAlong = -1f;

        public string bookSpineTexPath = "Things/Building/Furniture/GL_BookBox_book";

        /// <summary>bookSpacingAlong &lt; 0이면 0.155 사용.</summary>
        public bool UseVanillaBookSpacing => this.bookSpacingAlong < 0f;

        public float EffectiveSpacingAlong()
        {
            if (!this.UseVanillaBookSpacing)
            {
                return this.bookSpacingAlong;
            }

            return 0.155f;
        }

        /// <summary>일반 모드: 1권(슬롯1) 중심 = drawLoc + 이 값. 바닐라 모드: 바닐라 좌표에 더함.</summary>
        public Vector3 bookOriginOffset = new Vector3(-0.38f, 0.018292684f, 0.07f);

        /// <summary>일반 모드: 슬롯마다 더하는 벡터(슬롯1→2→…).</summary>
        public Vector3 bookAlongStep = new Vector3(0.155f, 0f, 0f);

        /// <summary>2층(슬롯 6~10)에 더하는 오프셋.</summary>
        public Vector3 secondFloorOffset = new Vector3(0f, 0.012f, -0.13f);

        public float bookScale = 1f;

        public int booksPerRow = 5;

        /// <summary>텍스처에 마스크가 있으면 true. false면 바닐라 책 VerticalGraphic과 같은 Cutout.</summary>
        public bool useCutoutComplexShader = false;

        /// <summary>책이 없을 때 VerticalGraphic drawSize를 못 쓰면 이 값(보조).</summary>
        public Vector2 bookSpineBaseDrawSize = new Vector2(0.8f, 0.8f);

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string err in base.ConfigErrors(parentDef))
            {
                yield return err;
            }

            if (this.booksPerRow <= 0)
            {
                yield return $"{parentDef.defName} CompProperties_GloomyBookshelfVisual: booksPerRow must be > 0.";
            }
        }
    }
}
