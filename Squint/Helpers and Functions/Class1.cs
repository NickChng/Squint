using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Threading.Tasks;
using System.Globalization;
using System.CodeDom;

namespace Squint
{
    //public abstract class ValueConverter<TSourceTarget> : ValueConverter<TSourceTarget, TSourceTarget> { }
    //public abstract class ValueConverter<TSource, TTarget> : ValueConverter<TSource, TTarget, object> { }
    public abstract class ValueConverter<TSource, TTarget, TParameter> : IValueConverter
    {
        protected abstract TTarget Convert(TSource value,
                                       Type targetType,
                                       TParameter parameter,
                                       CultureInfo culture);
        protected abstract TSource ConvertBack(TTarget value,
                                       Type targetType,
                                       TParameter parameter,
                                       CultureInfo culture);

        object IValueConverter.Convert(object value,
                                       Type targetType,
                                       object parameter,
                                       CultureInfo culture)
        {
            if (value != null && !(value is TSource))
                throw new InvalidCastException(string.Format("In order to use the generic IValueConverter you have to use the correct type. The passing type was {0} but the expected is {1}", value.GetType(), typeof(TSource)));
            if (parameter != null && !(parameter is TParameter))
                throw new InvalidCastException(string.Format("In order to use the generic IValueConverter you have to use the correct type as ConvertParameter. The passing type was {0} but the expected is {1}", parameter.GetType(), typeof(TParameter)));

            return Convert((TSource)value, targetType, (TParameter)parameter, culture);
        }

        object IValueConverter.ConvertBack(object value,
                                           Type targetType,
                                           object parameter,
                                           CultureInfo culture)
        {
            if (value != null && !(value is TTarget))
                throw new InvalidCastException(string.Format("In order to use the generic IValueConverter you have to use the correct type. The passing type was {0} but the expected is {1}", value.GetType(), typeof(TTarget)));
            if (parameter != null && !(parameter is TParameter))
                throw new InvalidCastException(string.Format("In order to use the generic IValueConverter you have to use the correct type as ConvertParameter. The passing type was {0} but the expected is {1}", parameter.GetType(), typeof(TParameter)));

            return ConvertBack((TTarget)value, targetType, (TParameter)parameter, culture);
        }
    }
    public static class Automapper
    {
        public static IMapper BeamGeometryDefinitionMapper;
        public static IMapper ProtocolMapper;
        public static IMapper ConstraintMapper;
        public static IMapper ProtocolStructureMapper;
        public static IMapper SquintMapper;

        public static void Initialize()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<DbBeamGeometry, BeamGeometryDefinition>());
            SquintMapper = config.CreateMapper();
            config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DbProtocol, Protocol>()
                    .ForMember(x=>x.Structures, opt=>opt.Ignore())
                    .ForMember(x=>x.ApprovalLevel, opt=>opt.MapFrom(y => y.DbApprovalLevel.ApprovalLevel)) // to my knowledge it's not possible to case this
                    .ForMember(x => x.ProtocolType, opt => opt.MapFrom(y => y.DbProtocolType.ProtocolType))
                    .ForMember(x => x.ApprovingUser, opt => opt.MapFrom(y => y.DbUser_Approver.ARIA_ID))
                    .ForMember(x => x.TreatmentSite, opt => opt.MapFrom(y => y.DbTreatmentCentre.TreatmentCentre))
                    .ForMember(x => x.TreatmentCentre, opt => opt.MapFrom(y => y.DbTreatmentCentre.TreatmentCentre))
                    .ForMember(x => x.Author, opt => opt.MapFrom(y => y.DbUser_ProtocolAuthor.ARIA_ID));
                cfg.CreateMap<DbProtocol, Protocol>().ForMember(x => x.Checklist, opt => opt.Ignore());
                cfg.CreateMap<DbBeamGeometry, BeamGeometryDefinition>();
                cfg.CreateMap<DbConstraint, Constraint>();
                cfg.CreateMap<DbProtocolStructure, ProtocolStructure>();
                cfg.CreateMap<double?, TrackedValue<double?>>().ConvertUsing(new TrackedValueConverter<double?>());
                cfg.CreateMap<double, TrackedValue<double>>().ConvertUsing(new TrackedValueConverter<double>());
                cfg.CreateMap<int, TrackedValue<int>>().ConvertUsing(new TrackedValueConverter<int>());
                cfg.CreateMap<int?, TrackedValue<int?>>().ConvertUsing(new TrackedValueConverter<int?>());
                cfg.CreateMap<int, ProtocolTypes>().ConvertUsing(new EnumConverter<ProtocolTypes>());
                cfg.CreateMap<int, ApprovalLevels>().ConvertUsing(new EnumConverter<ApprovalLevels>());
                cfg.CreateMap<int, TreatmentCentres>().ConvertUsing(new EnumConverter<TreatmentCentres>());
                cfg.CreateMap<int, TreatmentSites>().ConvertUsing(new EnumConverter<TreatmentSites>());
                cfg.CreateMap<int, TreatmentSites>().ConvertUsing(new EnumConverter<TreatmentSites>());
                cfg.CreateMap<TreatmentCentres, TrackedValue<TreatmentCentres>>().ConvertUsing(new TrackedValueConverter<TreatmentCentres>());
                cfg.CreateMap<TreatmentSites, TrackedValue<TreatmentSites>>().ConvertUsing(new TrackedValueConverter<TreatmentSites>());
                cfg.CreateMap<TreatmentIntents, TrackedValue<TreatmentIntents>>().ConvertUsing(new TrackedValueConverter<TreatmentIntents>());

            });
            //ProtocolMapper = config.CreateMapper();
            //config = new MapperConfiguration(cfg => cfg.CreateMap<DbConstraint, Constraint>());
            //ConstraintMapper = config.CreateMapper();
            //config = new MapperConfiguration(cfg => cfg.CreateMap<DbProtocolStructure, ProtocolStructure>());
            //ProtocolStructureMapper = config.CreateMapper();
        }

        public class TrackedValueConverter<T> : ITypeConverter<T, TrackedValue<T>> // ValueConverter<T,TrackedValue<T>>,
        {
            public TrackedValue<T> Convert(T source, TrackedValue<T> destination, ResolutionContext context)
            {
                return new TrackedValue<T>(source);
            }

            //protected override TrackedValue<T> Convert(T value, Type targetType, object parameter, CultureInfo culture)
            //{
            //    return new TrackedValue<T>(value);
            //}

            //protected override T ConvertBack(TrackedValue<T> value, Type targetType, object parameter, CultureInfo culture)
            //{
            //    throw new NotImplementedException();
            //}
        }

        public class EnumConverter<T> : ITypeConverter<int, T> where T: struct, Enum// ValueConverter<T,TrackedValue<T>>,
        {
            public T Convert(int source, T destination, ResolutionContext context)
            {
                Enum.TryParse<T>(source.ToString(), out T value);
                return value;
            }

            //protected override TrackedValue<T> Convert(T value, Type targetType, object parameter, CultureInfo culture)
            //{
            //    return new TrackedValue<T>(value);
            //}

            //protected override T ConvertBack(TrackedValue<T> value, Type targetType, object parameter, CultureInfo culture)
            //{
            //    throw new NotImplementedException();
            //}
        }

        //public interface TrackedValueConverter2<T> : ITypeConverter<T, TrackedValue<T>>
        //{
        //    TrackedValue<T> Convert(T source, TrackedValue<T> destination, ResolutionContext context, ResolutionContext contxt)
        //    {
        //        return new TrackedValue<T>(source);
        //    }
        //}
    }
}




