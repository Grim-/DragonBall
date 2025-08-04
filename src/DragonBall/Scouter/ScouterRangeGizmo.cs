using Verse;

namespace DragonBall
{
    public class ScouterRangeGizmo : Gizmo_Slider
    {
        private float currentRange;
        private float minRange;
        private float maxRange;
        private System.Action<float> onValueChanged;

        public ScouterRangeGizmo(float current, float min, float max, System.Action<float> onChange)
        {
            currentRange = current;
            minRange = min;
            maxRange = max;
            onValueChanged = onChange;
        }

        protected override float Target
        {
            get => ValuePercent;
            set
            {
                // Convert from percentage (0-1) back to actual range value
                float actualValue = minRange + (value * (maxRange - minRange));
                currentRange = actualValue;
                onValueChanged(actualValue);
            }
        }

        protected override float ValuePercent => (currentRange - minRange) / (maxRange - minRange);

        protected override string Title => $"Scan Range: {currentRange:F0}";

        protected override bool IsDraggable => true;

        protected override string GetTooltip() => $"Adjust scanning range between {minRange} and {maxRange} tiles";

        // Override this to use 0-1 range for internal slider
        protected override FloatRange DragRange => new FloatRange(0f, 1f);

        protected bool _DraggingBar = true;
        protected override bool DraggingBar { get => _DraggingBar; set => _DraggingBar = value; }
    }
}
