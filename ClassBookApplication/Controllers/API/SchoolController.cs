using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.School;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Service;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ClassBookApplication.Controllers.API
{
    [ApiVersion("1")]
    public class SchoolController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly ClassBookModelFactory _classBookModelFactory;



        #endregion

        #region Ctor

        public SchoolController(ClassBookManagementContext context,
            ClassBookService classBookService,
            ClassBookModelFactory classBookModelFactory)
        {
            this._context = context;
            this._classBookService = classBookService;
            this._classBookModelFactory = classBookModelFactory;
        }

        #endregion

        #region Register School

        // POST api/School/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                School schoolData = JsonConvert.DeserializeObject<School>(model.Data.ToString());
                if (schoolData != null)
                {
                    var singleUser = _context.Users.Where(x => x.Email == schoolData.Email).AsNoTracking();
                    if (!singleUser.Any())
                    {
                        (int SchoolId, string uniqueNo) = _classBookService.SaveSchool(schoolData, model.Files);
                        string UserName = schoolData.Name + uniqueNo;
                        //_classBookService.SaveMappingData((int)Module.School, SchoolId, schoolData.MappingRequestModel);
                        var user = _classBookService.SaveUserData(SchoolId, Module.School, UserName, schoolData.Email, model.FCMId, model.DeviceId);
                        var rest = _classBookService.RegisterMethod(model, "/api/v1/ChannelPartner/register");
                        await Task.Run(() => _classBookService.SendVerificationLinkEmail(schoolData.Email, user.Password, Module.School.ToString()));
                        responseModel.Message = ClassBookConstantString.Register_School_Success.ToString();
                        responseModel.Data = _classBookModelFactory.PrepareUserDetail(user);
                        return StatusCode((int)HttpStatusCode.OK, responseModel);
                    }
                    else
                    {
                        responseModel.Message = ClassBookConstantString.Validation_EmailExist.ToString();
                        return StatusCode((int)HttpStatusCode.Conflict, responseModel);
                    }
                }
                return StatusCode((int)HttpStatusCode.BadRequest);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }

        }

        // POST api/School/EditSchool
        [HttpPost("EditSchool")]
        public IActionResult EditSchool([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                School SchoolData = JsonConvert.DeserializeObject<School>(model.Data.ToString());
                if (SchoolData != null)
                {
                    if (_context.Users.Count(x => x.Email == SchoolData.Email && x.EntityId != SchoolData.Id) > 0)
                    {
                        responseModel.Message = ClassBookConstantString.Validation_EmailExist.ToString();
                        return StatusCode((int)HttpStatusCode.Conflict, responseModel);
                    }
                    else
                    {
                        var singleClass = _context.School.Where(x => x.Id == SchoolData.Id).AsNoTracking().FirstOrDefault();
                        int classId = _classBookService.UpdateSchool(SchoolData, singleClass, model.Files);
                        //_classBookService.SaveMappingData((int)Module.School, classId, SchoolData.MappingRequestModel);
                        responseModel.Message = ClassBookConstantString.Edit_School_Success.ToString();
                        return StatusCode((int)HttpStatusCode.OK, responseModel);
                    }
                }
                return StatusCode((int)HttpStatusCode.BadRequest);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }
        }

        #endregion

        #region GetSchoolDetails

        // GET api/School/GetAllSchool
        [HttpGet("GetAllSchool")]
        public IEnumerable<ListingModel> GetAllSchool()
        {
            return _classBookService.GetModuleDataByModuleId((int)Module.School);

        }

        // GET api/School/GetSchoolById/5
        [HttpGet("GetSchoolById/{id:int}")]
        public object GetSchoolById(int id)
        {
            var query = from school in _context.School
                        join state in _context.States on school.StateId equals state.Id
                        join city in _context.City on school.CityId equals city.Id
                        join pincode in _context.Pincode on school.Pincode equals pincode.Id
                        where school.Id == id && school.Active == true
                        orderby school.Id
                        select new
                        {
                            Name = school.Name,
                            Email = school.Email,
                            ContactNo = school.ContactNo,
                            AlternateContact = school.AlternateContact,
                            SchoolPhotoUrl = school.SchoolPhotoUrl,
                            EstablishmentDate = school.EstablishmentDate,
                            Address = school.Address,
                            TeachingExperience = school.TeachingExperience,
                            Description = school.Description,
                            RegistrationNo = school.RegistrationNo,
                            UniqueNo = school.UniqueNo,
                            StateName = state.Name,
                            CityName = city.Name,
                            Pincode = pincode.Name,
                        };
            var schoolData = query.FirstOrDefault();
            return schoolData;

        }

        // GET api/School/GetSchoolInformationByReferCode
        [HttpGet("GetSchoolInformationByReferCode")]
        public object GetSchoolInformationByReferCode(string referCode)
        {
            var query = from school in _context.School
                        where school.ReferCode == referCode && school.Active == true
                        select new
                        {
                            Name = school.Name,
                            UniqueId = school.UniqueNo
                        };
            return query.ToList();
        }
        #endregion


    }

    public string SaleOrderInternet(Order order)
    {
        try
        {
            // Fetch QRPayment record - AdviseTransaction response.
            var payAtVendorCode = string.Empty;
            var QRRecord = _orderService.GetQrPaymentRecordByNetworkTransactionID(order.AuthorizationTransactionId);
            if (QRRecord != null)
                payAtVendorCode = _orderService.GetSplitPaymentRecordByNetworkID(Convert.ToInt32(QRRecord.networkId)).VendorName;
            else
                payAtVendorCode = _orderService.GetSplitPaymentRecordByNetworkName(order.CardName).VendorName;

            List<string> empty = new List<string>();
            var paymentDay = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, "PaymentDayInternet", _storeContext.CurrentStore.Id);
            var customerInfo = _workContext.CurrentCustomer;
            //var addressInfo = _customerService.GetAddressesByCustomerId(customerInfo.Id);
            var shipAddress = _addressService.GetAddressById(order.ShippingAddressId ?? order.BillingAddressId);
            ManageResourceOrderRequest model = new ManageResourceOrderRequest();
            model.OrderNumber = "";
            model.ExternalId = "";
            model.Category = "B2C";

            LinkedCreditApplication linkedCreditApplication = new LinkedCreditApplication();
            linkedCreditApplication.Id = "";
            model.LinkedCreditApplication = linkedCreditApplication;

            SourceBusiness sourceBusiness = new SourceBusiness
            {
                AffiliateCode = "SelfService_SA",
                CountryCode = "zaf",
                Role = "Merchant"
            };
            model.SourceBusiness = sourceBusiness;

            List<Models.OrderNote> orderNote = new List<Models.OrderNote>();
            Models.OrderNote orderNoteModel = new Models.OrderNote();
            orderNoteModel.author = "";
            orderNoteModel.text = "";
            orderNote.Add(orderNoteModel);
            model.OrderNote = orderNote;
            model.ResubmitRequestIfNoStock = "true";

            BillTo billTo = new BillTo();
            billTo.Override = "false";
            billTo.Type = "Billing";

            Nop.Plugin.Misc.Checkout.Models.Customer customer = new Nop.Plugin.Misc.Checkout.Models.Customer();
            customer.FirstName = shipAddress.FirstName;
            customer.LastName = shipAddress.LastName;

            Contact contact = new Contact();
            contact.Email = customerInfo.Email;
            contact.Mobile = shipAddress.PhoneNumber;
            contact.Phone = shipAddress.PhoneNumber;
            customer.Contact = contact;

            if (customerInfo.ClarityCustomerID != null && customerInfo.ClarityCustomerID > 0)
                customer.Id = customerInfo.ClarityCustomerID.ToString();
            else
                customer.Id = "";

            customer.IDCardNumber = "";
            customer.IDType = "";

            if (customerInfo.ClarityAccountNumber != null && customerInfo.ClarityAccountNumber > 0)
                customer.FinancialAccountId = customerInfo.ClarityAccountNumber.ToString();
            else
                customer.FinancialAccountId = "";

            billTo.Customer = customer;

            Nop.Plugin.Misc.Checkout.Models.Address address = new Nop.Plugin.Misc.Checkout.Models.Address();
            address.BuildingName = shipAddress.Address2 == null ? string.Empty : shipAddress.Address2;
            address.City = shipAddress.City ?? string.Empty;
            address.Country = "South Africa";
            address.HouseNumber = shipAddress.ApartmentNumber == null ? string.Empty : shipAddress.ApartmentNumber;
            address.Locality = "";
            address.PostalCode = shipAddress.ZipPostalCode ?? string.Empty;
            address.PostBoxNumber = "";
            address.Province = shipAddress.StateProvinceId.ToString() ?? string.Empty;
            address.Street = shipAddress.Address1 ?? string.Empty;
            //address.StreetNr = "";
            address.StreetType = "";
            address.Suburb = shipAddress.Area ?? string.Empty;
            address.Locality = "";


            Location location = new Location();
            location.Href = "";
            location.Role = "";
            billTo.Location = location;
            model.BillTo = billTo;

            DeliverTo deliverTo = new DeliverTo();
            deliverTo.Location = location;
            BranchPlant branchPlant = new BranchPlant();

            if (order.PickupInStore)
            {
                branchPlant.PlantId = order.PlantId.ToString();
                branchPlant.Role = "Pickup";
                Nop.Plugin.Misc.Checkout.Models.Address addressForPlant = new Nop.Plugin.Misc.Checkout.Models.Address();
                Branch branch = _branchService.GetBranchByPlantId(order.PlantId);
                addressForPlant.BuildingName = branch.BuildingName;
                addressForPlant.HouseNumber = branch.UnitNumber.ToString();
                addressForPlant.PostalCode = branch.PostalCode.ToString();
                addressForPlant.City = branch.City;
                addressForPlant.PostBoxNumber = branch.PoBox.ToString();
                addressForPlant.Province = branch.Province;
                addressForPlant.Suburb = branch.Suburb;
                addressForPlant.Street = branch.StreetName.ToString();
                //addressForPlant.StreetNr = branch.StreetNumber.ToString();
                addressForPlant.StreetType = "";
                addressForPlant.Country = "South Africa";
                addressForPlant.Locality = "";
                deliverTo.Address = addressForPlant;
                billTo.Address = addressForPlant;
            }
            else
            {
                branchPlant.PlantId = "";
                branchPlant.Role = "";
                deliverTo.Address = address;
                billTo.Address = address;
            }
            deliverTo.Customer = customer;
            deliverTo.Override = "false";
            deliverTo.Type = "Delivery";
            deliverTo.Charges = empty;
            model.DeliverTo = deliverTo;
            billTo.BranchPlant = branchPlant;
            deliverTo.BranchPlant = branchPlant;

            List<NetPayment> netPayments = new List<NetPayment>();
            NetPayment netPayment = new NetPayment();
            netPayment.Currency = "ZAR";

            if (customerInfo.ClarityAccountNumber != null && customerInfo.ClarityAccountNumber > 0)
                netPayment.FinancialAccountId = customerInfo.ClarityAccountNumber.ToString();
            else
                netPayment.FinancialAccountId = "";

            netPayment.ModeOfPayment = payAtVendorCode + "|" + order.CardName;
            netPayment.OrderTotal = order.OrderTotal.ToString();
            netPayment.PaymentDate = order.PaidDateUtc?.ToString("yyyy-MM-dd");
            netPayment.PaymentReference = order.AuthorizationTransactionId == null ? "" : order.AuthorizationTransactionId.ToString();
            netPayment.SubscriptionPortion = "";
            netPayments.Add(netPayment);
            model.NetPayment = netPayments;
            DeliverTo deliverTonew = new DeliverTo();
            List<ResourceOrderItems> resourceOrderItems = new List<ResourceOrderItems>();
            ResourceOrderItems resourceOrderItem = new ResourceOrderItems();
            resourceOrderItem.Action = "add";
            resourceOrderItem.Quantity = "1";
            resourceOrderItem.DeliverTo = deliverTonew;

            List<OrderItemCharacteristic> orderItemCharacteristics = new List<OrderItemCharacteristic>();
            OrderItemCharacteristic itemCharacteristicModel = new OrderItemCharacteristic();
            itemCharacteristicModel.name = order.QuateProfileId != null ? "FLTE_FulfilledQuoteId" : "";
            itemCharacteristicModel.value = order.QuateProfileId != null ? order.QuateProfileId : "";
            itemCharacteristicModel.valueType = "";
            orderItemCharacteristics.Add(itemCharacteristicModel);
            //OrderItemCharacteristic itemCharacteristicModelRica = new OrderItemCharacteristic
            //{
            //    name = "FLTE_RICAStatus",
            //    value = "Pending",
            //    valueType = "string"
            //};
            //orderItemCharacteristics.Add(itemCharacteristicModelRica);
            resourceOrderItem.OrderItemCharacteristic = orderItemCharacteristics;

            Channel channel = new Channel();
            channel.mode = "";
            channel.name = "";
            resourceOrderItem.Channel = channel;
            resourceOrderItems.Add(resourceOrderItem);

            Resource resource = new Resource();
            resource.id = "";
            resource.Type = "DTH";

            IList<OrderItem> orderItemList = _orderService.GetOrderItems(order.Id);
            List<PrimaryResource> primaryResources = new List<PrimaryResource>();
            List<LinkedServices> linkedServices = new List<LinkedServices>();
            List<Bundle> bundles = new List<Bundle>();
            int count = 0;
            List<int> associatedProducts = new List<int>();
            var productIds = orderItemList.Select(x => x.ProductId);
            foreach (var item1 in productIds)
            {
                var associatedProduct = _productAttributeService.GetBundleAssociatedAttributeProduct(item1);
                if (associatedProduct > 0)
                    associatedProducts.Add(associatedProduct);
            }
            foreach (var orderItem in orderItemList)
            {
                var product = _productService.GetProductById(orderItem.ProductId);

                if (product.Name != "Sim Card")
                {
                    PrimaryResource primaryResourceModel = new PrimaryResource();
                    LinkedServices linkedServicesModel = new LinkedServices();
                    if (product.ResourceServiceType == (int)ResourceServiceType.PrimaryResource)
                    {
                        primaryResourceModel.Index = "0";
                        primaryResourceModel.InstallationRequired = "false";
                        primaryResourceModel.Href = "";
                        primaryResourceModel.ProductDescription = "";
                        primaryResourceModel.Model = "";
                        primaryResourceModel.BOM = product.SAPModelNumber;

                        RelatedParty relatedParty = new RelatedParty();
                        relatedParty.name = "";
                        relatedParty.role = "";
                        primaryResourceModel.RelatedParty = relatedParty;
                        primaryResourceModel.ResourceCharacteristic = empty;
                        primaryResourceModel.Type = "";

                        ItemPrice itemPrice = new ItemPrice();
                        itemPrice.Description = product.ShortDescription;
                        itemPrice.Name = product.Name;

                        Price price = new Price();
                        DutyFreeAmount dutyFreeAmount = new DutyFreeAmount();
                        dutyFreeAmount.Unit = "ZAR";
                        dutyFreeAmount.Value = orderItem.PriceInclTax.ToString();
                        price.DutyFreeAmount = dutyFreeAmount;

                        TaxIncludedAmount taxIncludedAmount = new TaxIncludedAmount();
                        taxIncludedAmount.Unit = "ZAR";
                        taxIncludedAmount.Value = orderItem.PriceInclTax.ToString();
                        price.TaxIncludedAmount = taxIncludedAmount;
                        price.TaxRate = "0";

                        itemPrice.Price = price;
                        itemPrice.PriceType = "NonRecurring";
                        primaryResourceModel.ItemPrice = itemPrice;

                        List<LinkedResources> LinkedResources = new List<LinkedResources>();
                        if (associatedProducts.Count > 0)
                        {
                            foreach (var associateProductItem in associatedProducts)
                            {
                                if (_productService.GetProductById(associatedProducts.FirstOrDefault()).Name != "Sim Card")
                                {
                                    var associatedPrd = _productService.GetProductById(associateProductItem);
                                    if (associatedPrd.ResourceServiceType == (int)ResourceServiceType.LinkedResource)
                                    {
                                        ItemPrice itemPriceForlinkedResourcesModel = new ItemPrice();
                                        itemPriceForlinkedResourcesModel.Description = associatedPrd.ShortDescription;
                                        itemPriceForlinkedResourcesModel.Name = associatedPrd.Name;

                                        Price priceforAssociateProduct = new Price();
                                        DutyFreeAmount dutyFreeAmountforAssociateProduct = new DutyFreeAmount();
                                        dutyFreeAmount.Unit = "ZAR";
                                        dutyFreeAmount.Value = orderItem.PriceInclTax.ToString();
                                        priceforAssociateProduct.DutyFreeAmount = dutyFreeAmountforAssociateProduct;

                                        TaxIncludedAmount taxIncludedAmountAssociated = new TaxIncludedAmount();
                                        taxIncludedAmountAssociated.Unit = "ZAR";
                                        taxIncludedAmountAssociated.Value = orderItem.PriceInclTax.ToString();
                                        priceforAssociateProduct.TaxIncludedAmount = taxIncludedAmountAssociated;
                                        priceforAssociateProduct.TaxRate = "0";

                                        itemPriceForlinkedResourcesModel.PriceType = "NonRecurring";
                                        itemPriceForlinkedResourcesModel.Price = priceforAssociateProduct;

                                        RelatedParty relatedPartyForlinkedResourcesModel = new RelatedParty();
                                        relatedPartyForlinkedResourcesModel.name = "";
                                        relatedPartyForlinkedResourcesModel.role = "";

                                        LinkedResources linkedResourcesModel = new LinkedResources();
                                        linkedResourcesModel.BOM = associatedPrd.SAPModelNumber;
                                        linkedResourcesModel.Href = "";
                                        linkedResourcesModel.InstallationRequired = "False";
                                        linkedResourcesModel.ItemPrice = itemPriceForlinkedResourcesModel;
                                        linkedResourcesModel.Model = associatedPrd.SAPModelNumber;
                                        linkedResourcesModel.ProductDescription = "";
                                        linkedResourcesModel.RelatedParty = relatedPartyForlinkedResourcesModel;
                                        linkedResourcesModel.ResourceCharacteristic = empty;
                                        linkedResourcesModel.Type = "";
                                        linkedResourcesModel.Index = "";
                                        LinkedResources.Add(linkedResourcesModel);
                                    }
                                }
                            }
                        }
                        primaryResourceModel.LinkedResources = LinkedResources;
                        primaryResources.Add(primaryResourceModel);
                    }
                    else if (product.ResourceServiceType == (int)ResourceServiceType.LinkedServices)
                    {
                        ItemPrice itemPriceForlinkedResourcesModel = new ItemPrice();
                        itemPriceForlinkedResourcesModel.Description = product.ShortDescription;
                        itemPriceForlinkedResourcesModel.Name = product.ProductSystemName;

                        Price priceLRM = new Price();
                        DutyFreeAmount dutyFreeAmountLMR = new DutyFreeAmount();
                        dutyFreeAmountLMR.Unit = "ZAR";
                        dutyFreeAmountLMR.Value = orderItem.PriceInclTax.ToString();
                        priceLRM.DutyFreeAmount = dutyFreeAmountLMR;

                        TaxIncludedAmount taxIncludedAmountLMR = new TaxIncludedAmount();
                        taxIncludedAmountLMR.Unit = "ZAR";
                        taxIncludedAmountLMR.Value = orderItem.PriceInclTax.ToString();
                        priceLRM.TaxIncludedAmount = taxIncludedAmountLMR;
                        priceLRM.TaxRate = "0";

                        itemPriceForlinkedResourcesModel.Price = priceLRM;
                        itemPriceForlinkedResourcesModel.PriceType = "NonRecurring";

                        RelatedParty relatedPartyForlinkedResourcesModel = new RelatedParty();
                        relatedPartyForlinkedResourcesModel.name = "";
                        relatedPartyForlinkedResourcesModel.role = "";

                        List<ServiceCharacteristic> serviceCharacteristics = new List<ServiceCharacteristic>();
                        ServiceCharacteristic serviceCharacteristicModel = new ServiceCharacteristic();
                        serviceCharacteristicModel.Name = "";
                        serviceCharacteristicModel.Value = "";
                        serviceCharacteristicModel.ValueType = "";
                        serviceCharacteristics.Add(serviceCharacteristicModel);
                        linkedServicesModel.ServiceCharacteristic = serviceCharacteristics;

                        ProductTerm productTerm = new ProductTerm();
                        productTerm.Description = "";
                        productTerm.Unit = "";
                        productTerm.Value = "";
                        linkedServicesModel.ProductTerm = productTerm;

                        List<Offers> offers = new List<Offers>();
                        Offers offersModel = new Offers();
                        offersModel.Id = "";
                        offersModel.Name = "";
                        offers.Add(offersModel);
                        linkedServicesModel.Offers = offers;

                        linkedServicesModel.Action = "add";
                        linkedServicesModel.AgreementType = "";
                        linkedServicesModel.AppliesToResourceIndex = empty;
                        linkedServicesModel.CommercialProductId = "";
                        linkedServicesModel.Desc = "";
                        linkedServicesModel.Href = "";
                        linkedServicesModel.Index = "";
                        linkedServicesModel.ProductTermMonths = "";
                        linkedServicesModel.ItemPrice = itemPriceForlinkedResourcesModel;
                        linkedServicesModel.Name = product.ProductSystemName;
                        linkedServicesModel.RelatedParty = relatedPartyForlinkedResourcesModel;
                        linkedServicesModel.State = "";
                        linkedServicesModel.SubscriptionFrequency = "";
                        linkedServicesModel.Type = "";
                        linkedServicesModel.VASCategory = "";
                        linkedServicesModel.IsCurrentPaymentFulfilled = "";
                        linkedServices.Add(linkedServicesModel);
                    }
                    count++;
                }
            }
            if (linkedServices.Count == 0)
            {
                List<LinkedServices> linkedServicesnew = new List<LinkedServices>();
                LinkedServices services = new LinkedServices();
                linkedServicesnew.Add(services);
                resource.LinkedServices = linkedServicesnew;
            }
            else
            {
                resource.LinkedServices = linkedServices;
            }
            resource.PrimaryResource = primaryResources;
            resource.Bundle = bundles;
            resourceOrderItem.Resource = resource;
            model.ResourceOrderItems = resourceOrderItems;

            string JsonResponse = string.Empty;

            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var Settings = _settingService.LoadSetting<DstvInternetSettings>(storeScope);
            string endpoint = Settings.ManageResourceApiURl;
            var serializestring = JsonConvert.SerializeObject(model, Formatting.Indented);

            //order note
            _orderService.InsertOrderNote(new Nop.Core.Domain.Orders.OrderNote
            {
                OrderId = order.Id,
                Note = "ManageResourceOrder Request for OrderId:" + "\n" + serializestring.ToString(),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            var client = new RestClient(endpoint);
            client.Timeout = -1;
            client.ClearHandlers();
            var jsonDeserializer = new JsonDeserializer();
            client.AddHandler("application/json", jsonDeserializer);


            var request = new RestRequest(Method.POST);
            request.AddParameter("application/json", serializestring, ParameterType.RequestBody);
            string token = _commonDstvService.GetToken();
            request.AddHeader("Authorization", "Bearer " + token);
            var queryResult = client.Execute(request);
            string json = queryResult.Content;
            var data = (JObject)JsonConvert.DeserializeObject(json);
            var ManageResourceOrderResJObject = data["ManageResourceOrderRes"];
            if (ManageResourceOrderResJObject != null)
            {
                var SalesOrdersJObject = ManageResourceOrderResJObject["SalesOrders"];
                var SAPOrderDetailJObject = SalesOrdersJObject["SAPOrderDetail"];
                string SAPOrderDetailSerialize = JsonConvert.SerializeObject(SAPOrderDetailJObject);
                var myDetails = JsonConvert.DeserializeObject<List<SAPOrderDetailModel>>(SAPOrderDetailSerialize);
                string referenceId = myDetails.FirstOrDefault().ReferenceId;
                string sapOrderId = myDetails.FirstOrDefault().SAPOrderId;
                order.SAPReferenceId = referenceId;
                order.SAPOrderId = sapOrderId;
                _orderService.UpdateOrder(order);

                if (string.IsNullOrEmpty(_workContext.CurrentCustomer.PaymentDayMode))
                {
                    var paymentDayMode = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, "PaymentDayMode");
                    _workContext.CurrentCustomer.PaymentDayMode = paymentDayMode;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
                _orderService.InsertOrderNote(new Nop.Core.Domain.Orders.OrderNote
                {
                    OrderId = order.Id,
                    Note = "Methode of payment for this order:" + _workContext.CurrentCustomer.PaymentDayMode,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }
            else
            {
                //order note
                _orderService.InsertOrderNote(new Nop.Core.Domain.Orders.OrderNote
                {
                    OrderId = order.Id,
                    Note = data.ToString(),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                return "/errorInternet";
            }
        }
        catch (Exception ex)
        {
            _orderService.InsertOrderNote(new Nop.Core.Domain.Orders.OrderNote
            {
                OrderId = order.Id,
                Note = "Exception for Subscribe Sales Order Response for OrderId:" + ex.Message.ToString() + "\n" + JsonConvert.SerializeObject(ex, Formatting.Indented).ToString(),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            return "/errorInternet";
        }
        return string.Empty;
    }
}
