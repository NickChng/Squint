using PropertyChanged;

namespace SquintScript
{

        [AddINotifyPropertyChangedInterface]
        public class BolusDefinition
        {
            public BolusDefinition(DbBolus DbB)
            {
                HU = new TrackedValue<double>(DbB.HU);
                Thickness = new TrackedValue<double>(DbB.Thickness);
                ToleranceThickness = new TrackedValue<double>(DbB.ToleranceThickness);
                ToleranceHU = new TrackedValue<double>(DbB.ToleranceHU);
                Indication = new TrackedValue<ParameterOptions>((ParameterOptions)DbB.Indication);
            }
            public TrackedValue<double> HU { get; set; }
            public TrackedValue<double> Thickness { get; set; }
            public TrackedValue<ParameterOptions> Indication { get; set; }
            public TrackedValue<double> ToleranceThickness { get; set; }
            public TrackedValue<double> ToleranceHU { get; set; }
        }

}

