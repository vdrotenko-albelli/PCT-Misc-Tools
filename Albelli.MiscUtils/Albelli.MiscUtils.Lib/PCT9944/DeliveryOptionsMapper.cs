using Albelli.CustomerCare.Backoffice.Models.Response;
using Microsoft.VisualBasic;
using CentiroModels = Centiro.PromiseEngine.Client;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public static class DeliveryOptionsMapper
    {
        public static CentiroModels.DeliveryRequest ToCentiroModel(this PromiseEngineDeliveryRequest request)
        {
            var deliveryOptions = request?.Options?.Select(o => o.ToCentiroModel()).ToList();
            var packages = request?.Packages?.Select(p => p.ToCentiroModel()).ToList();

            return new CentiroModels.DeliveryRequest
            {
                DeliveryId = request.DeliveryId != null ? Guid.Parse(request.DeliveryId) : Guid.NewGuid(),
                DeliveryOptions = deliveryOptions,
                Packages = packages
            };
        }

        public static CentiroModels.DeliveryOption ToCentiroModel(this DeliveryOption d)
        {
            var receiverAddress = d.ReceiverAddress?.ToCentiroModel();
            if (receiverAddress is not null)
                receiverAddress.AddressType = CentiroModels.AddressType.Receiver;

            var senderAddress = d.SenderAddress?.ToCentiroModel();
            if (senderAddress is not null)
                senderAddress.AddressType = CentiroModels.AddressType.Sender;

            return new CentiroModels.DeliveryOption
            {
                ReverseCalculation = d.IsReverseCalculation,
                DispatchDate = d.DispatchDate,
                SenderCode = d.SenderCode,
                Addresses = new List<CentiroModels.Address> { receiverAddress },
                Vendor = d.Vendor
            };
        }

        public static CentiroModels.Address ToCentiroModel(this Address a)
        {
            return new CentiroModels.Address
            {
                Name = a.Name,
                StreetAddress = a.Street,
                City = a.City,
                ZipCode = a.PostalCode?.Replace(" ", "") ?? "",
                IsoCountry = a.CountryCode
            };
        }

        public static CentiroModels.Package ToCentiroModel(this PackageInfo p)
        {
            var package = new CentiroModels.Package
            {
                UnitHeight = p.Package.HeightInMm,
                UnitLength = p.Package.LengthInMm,
                UnitWidth = p.Package.WidthInMm,
                UnitDimensionUnitOfMeasure = CentiroModels.DimensionUnitOfMeasure.Mm,
                UnitWeight = p.Package.WeightInGrams,
                UnitWeightUnitOfMeasure = CentiroModels.WeightUnitOfMeasure.G,
                Attributes = p?.AdditionalInfo?.Select(a => new CentiroModels.Attribute { Code = a.Code, Value = a.Value }).ToList(),
                Type = p.Package.Type
            };

            OrderDimensions(package);

            return package;
        }

        public static CentiroModels.CollectionPointRequest ToCentiroModel(this PickupPointsRequest r)
        {
            return new CentiroModels.CollectionPointRequest
            {
                NetworkId = r.NetworkId,
                Address = new CentiroModels.Address
                {
                    ZipCode = r.PostalCode,
                    IsoCountry = r.CountryCode
                },
                MaxCollectionPoints = r.MaxCollectionPoints,
                MaxDistance = r.MaxDistanceInKm,
                Latitude = r.Latitude,
                Longitude = r.Longitude
            };
        }

        //public static PickupPointsResponse ToAlbelliModel(this CentiroModels.CollectionPointResponse r)
        //{
        //    return new PickupPointsResponse
        //    {
        //        MapsUrl = r.MapsUrl,
        //        PickupPoints = r.CollectionPoints?.Select(p => p.ToAlbelliModel()).ToList()
        //    };
        //}

        //public static PickupPoint ToAlbelliModel(this CentiroModels.CollectionPoint p)
        //{
        //    return new PickupPoint
        //    {
        //        Id = p.Identifier,
        //        Distance = p.Distance,
        //        DistanceUnit = p.DistanceUnit,
        //        Name = p.Name,
        //        Address = new PickupPointAddress
        //        {
        //            Name = p.Address?.Name,
        //            Street = p.Address?.StreetAddress,
        //            Street2 = p.Address?.StreetAddressHouseNumber,
        //            City = p.Address?.City,
        //            PostalCode = p.Address?.ZipCode,
        //            CountryCode = p.Address?.IsoCountry,
        //            ExternalIdentifier = p.Address?.ExternalAddressIdentifier
        //        },
        //        OpeningTimes = p.OpeningTimes?.Select(t => t.ToAlbelliModel()).ToList(),
        //        Network = p.Network,
        //        GeoPoint = new GeoPoint
        //        {
        //            Latitude = p.GeoPoint?.Latitude,
        //            Longitude = p.GeoPoint?.Longitude
        //        }
        //    };
        //}

        public static OpeningTimes ToAlbelliModel(this CentiroModels.OpeningTimes t)
        {
            return new OpeningTimes
            {
                Day = (int)t.Day,
                OpenHours = t.OpenHours.Select(o => o.ToAlbelliModel()).ToList(),
            };
        }

        public static OpenHours ToAlbelliModel(this CentiroModels.OpenHours o)
        {
            return new OpenHours
            {
                OpeningTime = o.OpeningTime,
                ClosingTime = o.ClosingTime
            };
        }

        public static PromiseEngineDeliveryRequest ToDeliveryRequest(this CalculateCarrierModel m)
        {
            var isReverseCalculation = m?.EstimatedDeliveryDate.HasValue ?? false;

            DateTime? dispatchDate = isReverseCalculation ? m?.EstimatedDeliveryDate : m?.EstimatedShippingDate;

            return new PromiseEngineDeliveryRequest
            {
                DeliveryId = Guid.NewGuid().ToString(),
                Options = new List<DeliveryOption>
            {
                new DeliveryOption
                {
                    IsReverseCalculation = isReverseCalculation,
                    DispatchDate = new DateTime((dispatchDate ?? DateTime.UtcNow).Date.Ticks, DateTimeKind.Utc),
                    ReceiverAddress = new Address
                    {
                        PostalCode = m.ZipCode,
                        CountryCode = m.CountryId
                    },
                    SenderCode = SenderCodeHelper.GetCentiroSenderCode(m.PlantCode, m.Brand),
                    Vendor = m.Brand ?? string.Empty,
                    DeliveryTypes = m.DeliveryTypes,
                    DeliveryMethods = m.DeliveryMethods
                }
            },
                Packages = new List<PackageInfo>
            {
                new PackageInfo
                {
                    Package = m.Package
                }
            }
            };
        }

        //public static CarrierCalculationResult ToCalculateCarrierResponse(this CentiroModels.AvailableDeliveryOption deliveryOption)
        //{
        //    return new CarrierCalculationResult
        //    {
        //        CarrierName = deliveryOption.CarrierName,
        //        DeliveryType = deliveryOption.CarrierServiceType, // standard / express
        //        CarrierServiceKey = deliveryOption.CarrierServiceKey,
        //        // TODO: Stop using package label when Centiro adds CarrierServiceId property to ACSS
        //        CarrierServiceId = deliveryOption.PackageLabel,
        //        CarrierServiceCode = deliveryOption.ServiceCode,
        //        DeliveryMethod = deliveryOption.DeliveryMethod, // Home, PUDO
        //        NetworkId = deliveryOption.NetworkId,
        //        ModeOfTransport = deliveryOption.ModeOfTransport,
        //        InternalOnlyCarrier = false,
        //        Location = new CalculateCarrierResponseCarrierLocation
        //        {
        //            //todo: should set SapiLocationKey here instead of CalculateCarrierService.GetPromiseEngineDeliveryOptions ?
        //            //SapiLocationKey = ProviderKeyProvider.GetByPlantCode(model.PlantCode, Brand.IsVistaprintBrand(model.Brand)),
        //            Provider = deliveryOption.CarrierServiceKey,
        //            Currency = deliveryOption.Currency,
        //            ShippingLeadTime = 0, // hardcoded
        //            CarrierCode = deliveryOption.CarrierCodeLetter,
        //            ShipmentPriceIncludesVat = deliveryOption.ShipmentPriceIncludeVat ?? false,
        //            ShipmentPriceIncludesShippingCost = deliveryOption.ShipmentPriceIncludesShippingCost ?? false,
        //            ImportTaxRatio = deliveryOption.ImportTaxRatio ?? 1,
        //        },
        //        //hard-coded PNG 4x6 label for Vistaprint shipments from YPB (still via SAPI)
        //        PackageLabel = new DataAccess.Models.PackageLabel
        //        {
        //            LabelWidthInches = 4,
        //            LabelHeightInches = 6,
        //            LabelMediaType = Constants.ShippingLabelMediaTypes.PngImage
        //        },
        //        DeliveryDate = new DateTime(deliveryOption.DeliveryDate.Ticks, DateTimeKind.Utc),
        //        ShippingDate = deliveryOption.CutOffTime
        //    };
        //}

        public static void OrderDimensions(CentiroModels.Package package)
        {
            var dimensions = new List<double>
        {
            package.UnitLength,
            package.UnitWidth,
            package.UnitHeight
        };

            dimensions.Sort();

            package.UnitHeight = dimensions[0];
            package.UnitWidth = dimensions[1];
            package.UnitLength = dimensions[2];
        }
    }
}