﻿using GameManager.DB.Models;
using GameManager.Properties;
using GameManager.Services;
using Helper;
using Helper.Image;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GameManager.Components.Pages.components
{
    public partial class Card
    {
        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        private string ClassName => new CssBuilder(Class)
            .AddClass(IsSelected ? "selection" : "")
            .Build();

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        private IConfigService ConfigService { get; set; } = null!;

        [Parameter]
        public GameInfo? GameInfo { get; set; }

        [Parameter]
        public bool IsSelected { get; set; }

        [Parameter]
        public EventCallback<int> OnDeleteEventCallback { get; set; }

        [Parameter]
        public EventCallback<int> OnClick { get; set; }

        private List<string> DeveloperList
        {
            get
            {
                var list = GameInfo?.Developer?.Split(',').ToList();
                if (list == null || list.Count == 0)
                    return ["UnKnown"];
                return list;
            }
        }

        private string ImageSrc
        {
            get
            {
                if (GameInfo == null || string.IsNullOrEmpty(GameInfo.CoverPath))
                    return "/images/no-image.webp";
                return GameInfo.CoverPath.IsHttpLink()
                    ? GameInfo.CoverPath
                    : ImageHelper.GetDisplayUrl(ConfigService.GetCoverFullPath(GameInfo.CoverPath).Result!);
            }
        }

        private async Task OnEdit()
        {
            if (GameInfo == null)
                return;
            var inputModel = new DialogGameInfoEdit.FormModel();
            DataMapService.Map(GameInfo, inputModel);
            if (GameInfo.CoverPath != null && GameInfo.CoverPath.IsHttpLink())
                inputModel.Cover = GameInfo.CoverPath;
            else
                inputModel.Cover = ConfigService.GetCoverFullPath(GameInfo.CoverPath).Result;

            var parameters = new DialogParameters<DialogGameInfoEdit>
            {
                { x => x.Model, inputModel }
            };
            IDialogReference? dialogReference = await DialogService.ShowAsync<DialogGameInfoEdit>(@Resources.Dialog_Title_EditGameInfo,
                parameters,
                new DialogOptions
                {
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true,
                    BackdropClick = false
                });
            DialogResult? dialogResult = await dialogReference.Result;
            if (dialogResult.Canceled)
                return;
            if (dialogResult.Data is not DialogGameInfoEdit.FormModel resultModel)
                return;
            try
            {
                if ((resultModel.Cover == null && GameInfo.CoverPath != null)
                    || (resultModel.Cover.IsHttpLink() && !GameInfo.CoverPath.IsHttpLink()))
                {
                    await ConfigService.DeleteCoverImage(GameInfo.CoverPath);
                }
                else if (resultModel.Cover != null && (GameInfo.CoverPath.IsHttpLink() || GameInfo.CoverPath == null) &&
                         !resultModel.Cover.IsHttpLink())
                {
                    resultModel.Cover = await ConfigService.AddCoverImage(resultModel.Cover);
                }
                else if (resultModel.Cover != null &&
                         !resultModel.Cover.IsHttpLink())
                {
                    await ConfigService.ReplaceCoverImage(resultModel.Cover, GameInfo.CoverPath);
                }
            }
            catch (Exception e)
            {
                await DialogService.ShowMessageBox("Error", $"{e.Message}", cancelText: "Cancel");
            }

            DataMapService.Map(resultModel, GameInfo);
            await ConfigService.EditGameInfo(GameInfo);
            StateHasChanged();
        }

        private void OnOpenInExplorer()
        {
            Debug.Assert(GameInfo != null);
            if (GameInfo.ExePath == null)
                return;
            try
            {
                // using "explorer.exe" and send path
                Process.Start("explorer.exe", GameInfo.ExePath);
            }
            catch (Exception ex)
            {
                DialogService.ShowMessageBox("Error", ex.Message, cancelText:@Resources.Dialog_Button_Cancel);
            }
        }

        private async Task OnDelete()
        {
            if (GameInfo == null)
                return;
            if (OnDeleteEventCallback.HasDelegate)
                await OnDeleteEventCallback.InvokeAsync(GameInfo.Id);
        }

        private Task OnLaunch()
        {
            if (GameInfo == null || string.IsNullOrEmpty(GameInfo.ExePath) || !Directory.Exists(GameInfo.ExePath))
            {
                Snackbar.Add(@Resources.Message_NoExecutionFile, Severity.Warning);
                return Task.CompletedTask;
            }

            if (GameInfo.ExeFile is null or "Not Set")
            {
                Snackbar.Add(@Resources.Message_PleaseSetExeFirst, Severity.Warning);
                return Task.CompletedTask;
            }

            string executionFile = Path.Combine(GameInfo.ExePath, GameInfo.ExeFile);

            if (GameInfo.LaunchOption == null || GameInfo.LaunchOption?.LaunchWithLocaleEmulator == "None")
            {
                try
                {
                    var proc = new Process();
                    proc.StartInfo.FileName = executionFile;
                    proc.StartInfo.UseShellExecute = true;
                    bool runAsAdmin = GameInfo.LaunchOption is { RunAsAdmin: true };
                    if (runAsAdmin)
                    {
                        proc.StartInfo.Verb = "runas";
                    }

                    proc.Start();
                }
                catch (Exception e)
                {
                    Snackbar.Add($"Error: {e.Message}", Severity.Error);
                }

                return Task.CompletedTask;
            }

            AppSetting appSetting = ConfigService.GetAppSetting();
            string leConfigPath = Path.Combine(appSetting.LocaleEmulatorPath!, "LEConfig.xml");
            if (!File.Exists(leConfigPath))
            {
                Snackbar.Add(@Resources.Message_LENotFound, Severity.Error);
                return Task.CompletedTask;
            }

            var xmlDoc = XDocument.Load(leConfigPath);
            XElement? node =
                xmlDoc.XPathSelectElement(
                    $"//Profiles/Profile[@Name='{GameInfo.LaunchOption?.LaunchWithLocaleEmulator}']");
            XAttribute? guidAttr = node?.Attribute("Guid");
            if (guidAttr == null)
            {
                Snackbar.Add($"LE Config {GameInfo.LaunchOption!.LaunchWithLocaleEmulator} {Resources.Message_NotExist}",
                    Severity.Error);
                return Task.CompletedTask;
            }

            string guid = guidAttr.Value;
            string leExePath = Path.Combine(appSetting.LocaleEmulatorPath!, "LEProc.exe");
            try
            {
                var proc = new Process();
                proc.StartInfo.FileName = leExePath;
                proc.StartInfo.Arguments = $"-runas \"{guid}\" \"{executionFile}\"";
                proc.StartInfo.UseShellExecute = true;
                bool runAsAdmin = GameInfo.LaunchOption is { RunAsAdmin: true };
                if (runAsAdmin)
                {
                    proc.StartInfo.Verb = "runas";
                }

                proc.Start();
            }
            catch (Exception e)
            {
                Snackbar.Add($"Error: {e.Message}", Severity.Error);
            }

            return Task.CompletedTask;
        }

        private void OnCardClick()
        {
            if (GameInfo == null)
                return;
            if (OnClick.HasDelegate)
                OnClick.InvokeAsync(GameInfo.Id);
        }
    }
}