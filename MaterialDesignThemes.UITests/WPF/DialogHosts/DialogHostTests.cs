﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.UITests.Samples.DialogHost;
using MaterialDesignThemes.Wpf;
using XamlTest;
using Xunit;
using Xunit.Abstractions;

namespace MaterialDesignThemes.UITests.WPF.DialogHosts
{
    public class DialogHostTests : TestBase
    { 
        public DialogHostTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task OnOpenDialog_OverlayCoversContent()
        {
            await using var recorder = new TestRecorder(App);

            IVisualElement dialogHost = await LoadUserControl<WithCounter>();
            var overlay = await dialogHost.GetElement<Grid>("PART_ContentCoverGrid");

            var resultTextBlock = await dialogHost.GetElement<TextBlock>("ResultTextBlock");
            await Wait.For(async () => await resultTextBlock.GetText() == "Clicks: 0");

            var testOverlayButton = await dialogHost.GetElement<Button>("TestOverlayButton");
            await testOverlayButton.LeftClick();
            await Wait.For(async () => await resultTextBlock.GetText() == "Clicks: 1");

            var showDialogButton = await dialogHost.GetElement<Button>("ShowDialogButton");
            await showDialogButton.LeftClick();

            var closeDialogButton = await dialogHost.GetElement<Button>("CloseDialogButton");
            await Wait.For(async () => await closeDialogButton.GetIsVisible() == true);

            await testOverlayButton.LeftClick();
            await Wait.For(async () => await resultTextBlock.GetText() == "Clicks: 1");
            await Task.Delay(200);
            await closeDialogButton.LeftClick();

            var retry = new Retry(5, TimeSpan.FromSeconds(5));
            await Wait.For(async () => await overlay.GetVisibility() != Visibility.Visible, retry);
            await testOverlayButton.LeftClick();
            await Wait.For(async () => Assert.Equal("Clicks: 2", await resultTextBlock.GetText()), retry);

            recorder.Success();
        }

        [Fact]
        [Description("Issue 2282")]
        public async Task ClosingDialogWithIsOpenProperty_ShouldRaiseDialogClosingEvent()
        {
            await using var recorder = new TestRecorder(App);

            IVisualElement dialogHost = await LoadUserControl<ClosingEventCounter>();
            var showButton = await dialogHost.GetElement<Button>("ShowDialogButton");
            var closeButton = await dialogHost.GetElement<Button>("CloseButton");
            var resultTextBlock = await dialogHost.GetElement<TextBlock>("ResultTextBlock");

            await showButton.LeftClick();
            await Wait.For(async () => await closeButton.GetIsVisible());
            await Task.Delay(300);
            await closeButton.LeftClick();

            await Wait.For(async () => Assert.Equal("1", await resultTextBlock.GetText()));
            recorder.Success();
        }

        [Fact]
        [Description("Issue 2398")]
        public async Task FontSettingsSholdInheritIntoDialog()
        {
            await using var recorder = new TestRecorder(App);

            IVisualElement grid = await LoadXaml<Grid>(@"
<Grid TextElement.FontSize=""42""
      TextElement.FontFamily=""Times New Roman""
      TextElement.FontWeight=""ExtraBold"">
  <Grid.ColumnDefinitions>
    <ColumnDefinition />
    <ColumnDefinition />
  </Grid.ColumnDefinitions>
  <materialDesign:DialogHost>
    <materialDesign:DialogHost.DialogContent>
      <TextBlock Text=""Some Text"" x:Name=""TextBlock1"" />
    </materialDesign:DialogHost.DialogContent>
    <Button Content=""Show Dialog"" x:Name=""ShowButton1"" Command=""{x:Static materialDesign:DialogHost.OpenDialogCommand}"" />
  </materialDesign:DialogHost>
  <materialDesign:DialogHost Style=""{StaticResource MaterialDesignEmbeddedDialogHost}"" Grid.Column=""1"">
    <materialDesign:DialogHost.DialogContent>
      <TextBlock Text=""Some Text"" x:Name=""TextBlock2"" />
    </materialDesign:DialogHost.DialogContent>
    <Button Content=""Show Dialog"" x:Name=""ShowButton2"" Command=""{x:Static materialDesign:DialogHost.OpenDialogCommand}"" />
  </materialDesign:DialogHost>
</Grid>");
            var showButton1 = await grid.GetElement<Button>("ShowButton1");
            var showButton2 = await grid.GetElement<Button>("ShowButton2");
            
            await showButton1.LeftClick();
            await showButton2.LeftClick();
            await Task.Delay(300);

            var text1 = await grid.GetElement<TextBlock>("TextBlock1");
            var text2 = await grid.GetElement<TextBlock>("TextBlock2");

            Assert.Equal(42, await text1.GetFontSize());
            Assert.True((await text1.GetFontFamily())?.FamilyNames.Values.Contains("Times New Roman"));
            Assert.Equal(FontWeights.ExtraBold, await text1.GetFontWeight());

            Assert.Equal(42, await text2.GetFontSize());
            Assert.True((await text2.GetFontFamily())?.FamilyNames.Values.Contains("Times New Roman"));
            Assert.Equal(FontWeights.ExtraBold, await text2.GetFontWeight());

            recorder.Success();
        }

        [Fact]
        [Description("PR 2236")]
        public async Task ContentBackground_SetsDialogBackground()
        {
            await using var recorder = new TestRecorder(App);

            IVisualElement grid = await LoadXaml<Grid>(@"
<Grid>
  <Grid.ColumnDefinitions>
    <ColumnDefinition />
    <ColumnDefinition />
  </Grid.ColumnDefinitions>
  <materialDesign:DialogHost DialogBackground=""Red"" x:Name=""DialogHost1"">
    <materialDesign:DialogHost.DialogContent>
      <TextBlock Text=""Some Text"" x:Name=""TextBlock1"" />
    </materialDesign:DialogHost.DialogContent>
    <Button Content=""Show Dialog"" x:Name=""ShowButton1"" Command=""{x:Static materialDesign:DialogHost.OpenDialogCommand}"" />
  </materialDesign:DialogHost>
  <materialDesign:DialogHost Style=""{StaticResource MaterialDesignEmbeddedDialogHost}"" DialogBackground=""Red"" x:Name=""DialogHost2"" Grid.Column=""1"">
    <materialDesign:DialogHost.DialogContent>
      <TextBlock Text=""Some Text"" x:Name=""TextBlock2"" />
    </materialDesign:DialogHost.DialogContent>
    <Button Content=""Show Dialog"" x:Name=""ShowButton2"" Command=""{x:Static materialDesign:DialogHost.OpenDialogCommand}"" />
  </materialDesign:DialogHost>
</Grid>");
            var showButton1 = await grid.GetElement<Button>("ShowButton1");
            var showButton2 = await grid.GetElement<Button>("ShowButton2");

            await showButton1.LeftClick();
            await showButton2.LeftClick();

            var dialogHost1 = await grid.GetElement<DialogHost>("DialogHost1");
            var dialogHost2 = await grid.GetElement<DialogHost>("DialogHost2");

            await Wait.For(async () =>
            {
                var card1 = await dialogHost1.GetElement<Card>("PART_PopupContentElement");
                var card2 = await dialogHost2.GetElement<Card>("PART_PopupContentElement");

                Assert.Equal(Colors.Red, await card1.GetBackgroundColor());
                Assert.Equal(Colors.Red, await card2.GetBackgroundColor());
            });
        }
    }
}
