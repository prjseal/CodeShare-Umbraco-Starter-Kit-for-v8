using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PackageActions;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace CSUSK.Core.PackageActions
{
    public class CreateMediaHandler : IPackageAction
    {
        IMediaService _mediaService = Current.Services.MediaService;

        public string Alias()
        {
            return "CreateMediaHandler";
        }

        public bool Execute(string packageName, XElement xmlData)
        {
            var contentService = Current.Services.ContentService;
            var mediaTypeService = Current.Services.MediaTypeService;
            var mediaService = Current.Services.MediaService;
            var dataTypeService = Current.Services.DataTypeService;
            var fileService = Current.Services.FileService;

            try
            {
                Current.Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().GetType(), "Executing CreateMediaHandler");
                return CreateMediaItems();
            }
            catch (Exception ex)
            {
                Current.Logger.Error(System.Reflection.MethodBase.GetCurrentMethod().GetType(), "INSTALL Package Error", ex);
                return false;
            }
        }

        public bool CreateMediaItems()
        {
            var contentService = Current.Services.ContentService;
            var mediaTypeService = Current.Services.MediaTypeService;
            var mediaService = Current.Services.MediaService;
            var dataTypeService = Current.Services.DataTypeService;
            var fileService = Current.Services.FileService;
            var contentTypeBaseServiceProvider = Current.Services.ContentTypeBaseServices;

            const string SLIDE_IMAGE_ALIAS = "slideImage";
            const string MAIN_IMAGE_ALIAS = "mainImage";

            try
            {
                int parentId = -1;
                IMedia codeImage = CreateMediaItem(parentId, "code.jpg", contentTypeBaseServiceProvider);
                IMedia carouselImage = CreateMediaItem(parentId, "carousel.jpg", contentTypeBaseServiceProvider);
                IMedia navigationImage = CreateMediaItem(parentId, "navigation.jpg", contentTypeBaseServiceProvider);
                IMedia partyImage = CreateMediaItem(parentId, "party.jpg", contentTypeBaseServiceProvider);

                Guid homeId = new Guid("a53c075a-61a2-456e-a73d-e65e52efea92");
                IContent homePage = contentService.GetById(homeId);
                string carouselData = homePage.GetValue<string>("mainCarousel");

                JArray carouselObject = JArray.Parse(carouselData);

                carouselObject[0][SLIDE_IMAGE_ALIAS] = codeImage.GetUdi().ToString();
                carouselObject[1][SLIDE_IMAGE_ALIAS] = navigationImage.GetUdi().ToString();
                carouselObject[2][SLIDE_IMAGE_ALIAS] = carouselImage.GetUdi().ToString();

                homePage.SetValue("mainCarousel", carouselObject.ToString());
                contentService.SaveAndPublish(homePage);

                //get about page
                Guid aboutPageId = new Guid("67ab09ce-31cf-43b4-97ca-1bc567376a42");
                IContent aboutPage = contentService.GetById(aboutPageId);
                aboutPage.SetValue(MAIN_IMAGE_ALIAS, partyImage.GetUdi().ToString());
                contentService.Save(aboutPage);
                
                //get article 1
                Guid article1Id = new Guid("ae01b184-a845-48f2-a0bd-a2254d02857e");
                IContent article1 = contentService.GetById(article1Id);
                article1.SetValue(MAIN_IMAGE_ALIAS, partyImage.GetUdi().ToString());
                contentService.Save(article1);

                //get article 2
                Guid article2Id = new Guid("580d0738-ba3b-447c-8907-a3a04b6af3e3");
                IContent article2 = contentService.GetById(article2Id);
                article2.SetValue(MAIN_IMAGE_ALIAS, codeImage.GetUdi().ToString());
                contentService.Save(article2);

                //get article 3
                Guid article3Id = new Guid("4e35da7e-8aa4-40ba-aa11-ee1b91f664bd");
                IContent article3 = contentService.GetById(article3Id);
                article3.SetValue(MAIN_IMAGE_ALIAS, carouselImage.GetUdi().ToString());
                contentService.Save(article3);


                if(homePage != null)
                {
                    contentService.SaveAndPublishBranch(homePage, true);
                }

                Current.Logger.Info(System.Reflection.MethodBase.GetCurrentMethod().GetType(), "Created Media Items");
                return true;
            }
            catch (Exception ex)
            {
                Current.Logger.Error(System.Reflection.MethodBase.GetCurrentMethod().GetType(), "Error when creating media items.", ex);
                return false;
            }
        }

        private IMedia CreateMediaItem(int parentId, string fileName, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
        {
            IMedia newFile = _mediaService.CreateMedia(fileName, parentId, "Image");
            string filePath = HttpContext.Current.Server.MapPath("~/img/" + fileName);
            using (FileStream stream = System.IO.File.Open(filePath, FileMode.Open))
            {
                newFile.SetValue(contentTypeBaseServiceProvider, "umbracoFile", fileName, stream);
            }
            _mediaService.Save(newFile);
            return newFile;
        }

        public XmlNode SampleXml()
        {
            const string sample = "<Action runat=\"install\" undo=\"true\" alias=\"CreateMediaHandler\"></Action>";
            return ParseStringToXmlNode(sample);
        }

        private static XmlNode ParseStringToXmlNode(string value)
        {
            var xmlDocument = new XmlDocument();
            var xmlNode = AddTextNode(xmlDocument, "error", "");

            try
            {
                xmlDocument.LoadXml(value);
                return xmlDocument.SelectSingleNode(".");
            }
            catch
            {
                return xmlNode;
            }
        }

        private static XmlNode AddTextNode(XmlDocument xmlDocument, string name, string value)
        {
            var node = xmlDocument.CreateNode(XmlNodeType.Element, name, "");
            node.AppendChild(xmlDocument.CreateTextNode(value));
            return node;
        }

        public bool Undo(string packageName, XElement xmlData)
        {
            //Can't really undo this.
            return true;
        }
    }
}