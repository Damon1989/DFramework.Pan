﻿using Abp.Application.Navigation;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.Threading;
using DFramework.Pan.Web.Models.Layout;
using System.Web.Mvc;

namespace DFramework.Pan.Web.Controllers
{
    public class LayoutController : PanControllerBase
    {
        private readonly IUserNavigationManager _userNavigationManager;
        private readonly ILanguageManager _languageManager;

        public LayoutController(
            IUserNavigationManager userNavigationManager,
            ILocalizationManager localizationManager,
            ILanguageManager languageManager)
        {
            _userNavigationManager = userNavigationManager;
            _languageManager = languageManager;
        }

        [ChildActionOnly]
        public PartialViewResult TopMenu(string activeMenu = "")
        {
            var model = new TopMenuViewModel
            {
                MainMenu = AsyncHelper.RunSync(() => _userNavigationManager.GetMenuAsync("MainMenu", AbpSession.ToUserIdentifier())),
                ActiveMenuItemName = activeMenu
            };

            return PartialView("_TopMenu", model);
        }

        [ChildActionOnly]
        public PartialViewResult LanguageSelection()
        {
            var model = new LanguageSelectionViewModel
            {
                CurrentLanguage = _languageManager.CurrentLanguage,
                Languages = _languageManager.GetLanguages()
            };

            return PartialView("_LanguageSelection", model);
        }
    }
}