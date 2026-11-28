using System;

namespace ImperiumOfMan
{
    // Pure viewport geometry. All values are logical canvas units, apart
    // from the physical screen size and Scale. No Unity dependencies.
    internal sealed class CraftingViewportLayout
    {
        internal const int MinUiPercent = 70, MaxUiPercent = 150;
        internal const float Outer = 8f, Gap = 8f, ToolWidth = 650f, FilterRight = 640f;
        internal float Scale, Width, Height, GraphTop, GraphBottom, GraphRight, InspectorWidth, InspectorHeight;
        internal bool Side, SharedTools;
        internal float GraphWidth => Width - Gap - GraphRight - 4f;
        internal float GraphHeight => Height - GraphTop - GraphBottom - 4f;

        internal static CraftingViewportLayout Calculate(int pixelsWide, int pixelsHigh, int uiPercent, bool inspector)
        {
            float w = Math.Max(1, pixelsWide), h = Math.Max(1, pixelsHigh);
            float scale = Math.Min(w / 1280f, h / 720f) * Math.Max(MinUiPercent, Math.Min(MaxUiPercent, uiPercent)) / 100f;
            var result = new CraftingViewportLayout
            {
                Scale = scale, Width = w / scale - Outer * 2, Height = h / scale - Outer * 2
            };
            result.SharedTools = result.Width >= FilterRight + ToolWidth + 2 * Gap;
            result.GraphTop = result.SharedTools ? 70 : 102;
            result.Side = inspector && w / h >= 2.0f && result.Width >= 1100;
            result.GraphRight = result.GraphBottom = Gap;
            if (inspector)
            {
                if (result.Side)
                {
                    result.InspectorWidth = Math.Max(340, Math.Min(500, result.Width * 0.23f));
                    result.InspectorHeight = result.Height - result.GraphTop - Gap;
                    result.GraphRight = result.InspectorWidth + 2 * Gap;
                }
                else
                {
                    result.InspectorHeight = Math.Max(124, Math.Min(190, result.Height * 0.25f));
                    result.InspectorWidth = result.Width - Gap * 2;
                    result.GraphBottom = result.InspectorHeight + 2 * Gap;
                }
            }
            return result;
        }

        internal static float FitZoom(float viewWidth, float viewHeight, float graphWidth, float graphHeight)
        {
            float x = Math.Max(1, viewWidth - 24f) / Math.Max(1, graphWidth);
            float y = Math.Max(1, viewHeight - 24f) / Math.Max(1, graphHeight);
            return Math.Max(0.02f, Math.Min(1.1f, Math.Min(x, y)));
        }
    }
}
