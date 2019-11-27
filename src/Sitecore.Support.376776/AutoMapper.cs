using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Form.Core.Utility;
using Sitecore.Forms.Core.Data;
using Sitecore.Forms.Mvc.Helpers;
using Sitecore.Forms.Mvc.Interfaces;
using Sitecore.Forms.Mvc.ViewModels;

namespace Sitecore.Support.Forms.Mvc.Services
{
    public class AutoMapper : Sitecore.Forms.Mvc.Services.AutoMapper, IAutoMapper<IFormModel, FormViewModel>
    {
        FormViewModel IAutoMapper<IFormModel, FormViewModel>.GetView(IFormModel formModel)
        {
            Assert.ArgumentNotNull(formModel, "formModel");
            FormViewModel formViewModel = new FormViewModel
            {
                UniqueId = formModel.UniqueId,
                Information = (formModel.Item.Introduction ?? string.Empty),
                IsAjaxForm = formModel.Item.IsAjaxMvcForm,
                IsSaveFormDataToStorage = formModel.Item.IsSaveFormDataToStorage,
                Title = (formModel.Item.FormName ?? string.Empty),
                Name = (formModel.Item.FormName ?? string.Empty),
                TitleTag = formModel.Item.TitleTag.ToString(),
                ShowTitle = formModel.Item.ShowTitle,
                ShowFooter = formModel.Item.ShowFooter,
                ShowInformation = formModel.Item.ShowIntroduction,
                SubmitButtonName = (formModel.Item.SubmitName ?? string.Empty),
                SubmitButtonPosition = (formModel.Item.SubmitButtonPosition ?? string.Empty),
                SubmitButtonSize = (formModel.Item.SubmitButtonSize ?? string.Empty),
                SubmitButtonType = (formModel.Item.SubmitButtonType ?? string.Empty),
                SuccessMessage = (formModel.Item.SuccessMessage ?? string.Empty),
                SuccessSubmit = false,
                Errors = (from x in formModel.Failures
                    select x.ErrorMessage).ToList(),
                Visible = true,
                LeftColumnStyle = formModel.Item.LeftColumnStyle,
                RightColumnStyle = formModel.Item.RightColumnStyle,
                Footer = formModel.Item.Footer,
                Item = formModel.Item.InnerItem,
                FormType = formModel.Item.FormType,
                ReadQueryString = formModel.ReadQueryString,
                QueryParameters = formModel.QueryParameters
            };
            formViewModel.CssClass = ((formModel.Item.FormTypeClass ?? string.Empty) + " " + (formModel.Item.CustomCss ?? string.Empty) + " " + (formModel.Item.FormAlignment ?? string.Empty)).Trim();
            ReflectionUtils.SetXmlProperties(formViewModel, formModel.Item.Parameters, ignoreErrors: true);
            formViewModel.Sections = (from x in formModel.Item.SectionItems
                select GetSectionViewModelEx(new SectionItem(x), formViewModel) into x
                where x != null
                select x).ToList();
            return formViewModel;
        }

        protected SectionViewModel GetSectionViewModelEx(SectionItem item, FormViewModel formViewModel)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(formViewModel, "formViewModel");
            SectionViewModel sectionViewModel = new SectionViewModel
            {
                Fields = new List<FieldViewModel>(),
                Item = item.InnerItem
            };
            string title = item.Title;
            sectionViewModel.Visible = true;
            if (!string.IsNullOrEmpty(title))
            {
                sectionViewModel.ShowInformation = true;
                sectionViewModel.Title = (item.Title ?? string.Empty);
                ReflectionUtils.SetXmlProperties(sectionViewModel, item.Parameters, ignoreErrors: true);
                sectionViewModel.ShowTitle = (sectionViewModel.ShowLegend != "No");
                ReflectionUtils.SetXmlProperties(sectionViewModel, item.LocalizedParameters, ignoreErrors: true);
            }
            sectionViewModel.Fields = (from x in item.Fields
                select GetFieldViewModel(x, formViewModel) into x
                where x != null
                select x).ToList();
            if (!string.IsNullOrEmpty(item.Conditions))
            {
                RulesManager.RunRules(item.Conditions, sectionViewModel);
            }

            if (!sectionViewModel.Visible)
            {
                sectionViewModel.Fields.ForEach(f => f.Visible = false);
            }

            return sectionViewModel;
          
        }
    }
}