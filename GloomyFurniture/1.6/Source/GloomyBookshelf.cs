using RimWorld;
using UnityEngine;
using Verse;

namespace Gloomylynx
{
    /// <summary>
    /// 글루미 책장: 남향일 때만 2열(1층 5권·2층 5권) 등 이미지를 그림. XML로 위치·간격·스케일 조절.
    /// </summary>
    public class Building_GloomyBookshelf : Building_Bookcase
    {
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            drawLoc -= Altitudes.AltIncVect * 2f;

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
                Vector3 rowShift = i >= p.booksPerRow ? p.secondFloorOffset : Vector3.zero;
                Vector3 loc = drawLoc + p.firstFloorStart + p.bookAlongStep * slotInRow + rowShift;
                spine.Draw(loc, bookRot, this, 0f);
            }
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

        public CompProperties_GloomyBookshelfVisual Props => (CompProperties_GloomyBookshelfVisual)this.props;

        public Graphic SpineGraphic
        {
            get
            {
                if (this.spineGraphicInt == null && !string.IsNullOrEmpty(this.Props.bookSpineTexPath))
                {
                    Vector2 drawSize = this.Props.BaseDrawSize * this.Props.bookScale;
                    this.spineGraphicInt = GraphicDatabase.Get<Graphic_Single>(
                        this.Props.bookSpineTexPath,
                        ShaderDatabase.CutoutComplex,
                        drawSize,
                        this.parent.DrawColor);
                }

                return this.spineGraphicInt;
            }
        }

        public void InvalidateSpineGraphic()
        {
            this.spineGraphicInt = null;
        }
    }

    public class CompProperties_GloomyBookshelfVisual : CompProperties
    {
        public CompProperties_GloomyBookshelfVisual()
        {
            this.compClass = typeof(Comp_GloomyBookshelfVisual);
        }

        /// <summary>책 등 텍스처 경로(모드 루트 기준).</summary>
        public string bookSpineTexPath = "Things/Building/Furniture/GL_BookBox_book";

        /// <summary>1층(아랫줄) 첫 권(슬롯 1) 중심의 drawLoc 기준 오프셋.</summary>
        public Vector3 firstFloorStart = new Vector3(-0.38f, 0.018292684f, 0.07f);

        /// <summary>같은 층에서 슬롯 n → n+1로 갈 때 더하는 벡터(좌표 양수 방향으로 쌓이게 XML에서 맞춤).</summary>
        public Vector3 bookAlongStep = new Vector3(0.152f, 0f, 0f);

        /// <summary>2층(윗줄) 기준점 = 1층과 동일한 슬롯 인덱스일 때 1층 대비 추가 오프셋(678910 행).</summary>
        public Vector3 secondFloorOffset = new Vector3(0f, 0.012f, -0.13f);

        /// <summary>등 이미지 drawSize에 곱하는 배율.</summary>
        public float bookScale = 1f;

        /// <summary>한 줄 최대 권수(기본 5). 1층 0..booksPerRow-1, 그 다음 층은 +secondFloorOffset.</summary>
        public int booksPerRow = 5;

        /// <summary>스케일 적용 전 등 기본 크기(가로·세로).</summary>
        public Vector2 bookSpineBaseDrawSize = new Vector2(0.18f, 0.5f);

        public Vector2 BaseDrawSize => this.bookSpineBaseDrawSize;
    }
}
