using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Material.Icons.Avalonia;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.MessageBox;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs
{
    public partial class DialogsViewModel
    {
        [RelayCommand]
        private void OpenMessageBoxStyleDialog()
        {
            _dialogManager.CreateDialog()
                .OfType(SelectedType)
                .WithTitle("MessageBox")
                .WithContent("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.")
                .WithActionButton("Close " + SelectedType.ToString(), _ => { }, true, "Flat")
                .Dismiss().ByClickingBackground()
                .TryShow();

            toastManager.CreateToast()
                    .WithTitle("Dialog opened")
                    .WithContent($"Dialog opened")
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private async Task OpenAsyncMessageBoxStyleDialog()
        {
            var task = _dialogManager.CreateDialog()
                 .OfType(SelectedType)
                 .WithTitle("Async blocking MessageBox")
                 .WithContent("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.")
                 .WithYesNoResult("Yes", "No")
                 .Dismiss().ByClickingBackground()
                 .TryShowAsync();

            toastManager.CreateToast()
                .WithTitle("Dialog opened")
                .WithContent($"{SelectedType} opened")
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();

            var result = await task;

            toastManager.CreateToast()
                    .WithTitle("Dialog closed")
                    .WithContent($"{SelectedType} DialogResult: {result}")
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
        }

        [RelayCommand]
        private async Task OpenSimpleQuestionMessageBox()
        {
            var result = await SukiMessageBox.ShowDialog(new SukiMessageBoxHost
            {
                IconPreset = SukiMessageBoxIcons.Question,
                Content = "Are you sure you want to process this action?",
                ActionButtonsPreset = SukiMessageBoxButtons.YesNo
            });

            toastManager.CreateToast()
                    .WithTitle("Dialog Option Clicked")
                    .WithContent($"You clicked option: {result}")
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
        }

        [RelayCommand]
        private async Task OpenAdvancedQuestionMessageBox()
        {
            var result = await SukiMessageBox.ShowDialog(new SukiMessageBoxHost
            {
                IconPreset = SelectedMessageBoxIcon,
                Header = "Reformat file",
                Content = "Are you sure you want to process this action?",
                ActionButtonsPreset = SelectedMessageBoxButtons,
                FooterLeftItemsSource = [
                    SukiMessageBoxButtonsFactory.CreateButton("About"),
                ]
            });

            toastManager.CreateToast()
                .WithTitle("Dialog Option Clicked")
                .WithContent($"You clicked option: {result}")
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        }

        [RelayCommand]
        private async Task OpenCustomMarkdownMessageBox()
        {
            var autoUpgradeButton = SukiMessageBoxButtonsFactory.CreateButton("Auto upgrade", SukiMessageBoxResult.Yes, "Flat Accent");
            var manualUpgradeButton = SukiMessageBoxButtonsFactory.CreateButton("Manual upgrade");
            var cancelButton = SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.Cancel);

            var result = await SukiMessageBox.ShowDialog(new SukiMessageBoxHost
            {
                IconPresetSize = 32,
                IconPreset = SukiMessageBoxIcons.Star,
                UseAlternativeHeaderStyle = UseAlternativeHeaderStyle,
                ShowHeaderContentSeparator = ShowHeaderContentSeparator,
                Header = "Changelog - Version 2.5.0",
                Content = new Markdown.Avalonia.MarkdownScrollViewer()
                {
                    Markdown = """
                           ## New Features:

                           - Added dark mode support for improved user experience in low-light environments.
                           - Implemented multi-language support, including Spanish, French, and German.
                           - Introduced a new dashboard with enhanced analytics and real-time insights.
                           - Added an in-app feedback system to gather user suggestions and bug reports.

                           ## Improvements:

                           - Optimized application startup time, reducing load time by 30%.
                           - Enhanced security measures, including two-factor authentication (2FA) support.
                           - Improved search functionality with better filtering and sorting options.
                           - Revamped user interface for a cleaner and more modern look.

                           ## Bug Fixes:

                           - Fixed an issue where notifications were not being displayed correctly.
                           - Resolved a crash that occurred when uploading large files.
                           - Corrected language translation errors in certain sections of the app.
                           - Fixed a bug where saved preferences were not being retained after updates.

                           ## Deprecations:

                           - Removed support for Android 6.0 due to security concerns and low adoption.
                           - Deprecated the legacy reporting module; users are encouraged to use the new analytics dashboard.

                           ## Known Issues:

                           - Some users may experience minor UI misalignment in landscape mode; a fix is in progress.
                           - Occasional delays in syncing data with cloud storage under poor network conditions.

                           For any feedback or support, please reach out to our support team at support@example.com.
                           """
                },
                ActionButtonsSource = [autoUpgradeButton, manualUpgradeButton, cancelButton]
            }, new SukiMessageBoxOptions
            {
                UseNativeWindow = UseNativeWindow
            });

            if (result is SukiMessageBoxResult.Yes)
            {
                // Do auto upgrade
            }
            else if (ReferenceEquals(result, manualUpgradeButton))
            {
                var launcher = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!).MainWindow)!.Launcher;
                await launcher.LaunchUriAsync(new Uri("https://github.com/kikipoulet/SukiUI"));
            }

            var resultText = result?.ToString();
            if (result is Button button) resultText = button.Content?.ToString();

            toastManager.CreateToast()
                .WithTitle("Dialog Option Clicked")
                .WithContent($"You clicked option: {resultText}")
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        }

        [RelayCommand]
        private async Task OpenAnimatedHeaderMessageBox()
        {
            var icon = new MaterialIcon
            {
                Kind = MaterialIconKind.Heart,
                Foreground = Brushes.DeepPink,
                Width = 10,
                Height = 10,
            };

            var header = new SelectableTextBlock()
            {
                Text = "Thank you!"
            };

            icon.Animate(Layoutable.WidthProperty, 10d, 42d, TimeSpan.FromSeconds(2));
            icon.Animate(Layoutable.HeightProperty, 10d, 42d, TimeSpan.FromSeconds(2));
            header.Animate(TextBlock.FontSizeProperty, 10d, 28d, TimeSpan.FromSeconds(2));

            var result = await SukiMessageBox.ShowDialog(new SukiMessageBoxHost
            {
                UseAlternativeHeaderStyle = true,
                Icon = icon,
                Header = header,
                Content = "We sincerely appreciate your continued support and feedback. Your input helps us improve and create a better experience for everyone. Thank you for being a valued part of our community!",
                ActionButtonsPreset = SukiMessageBoxButtons.Close,
            }, new SukiMessageBoxOptions()
            {
                Title = "SukiUI Thanks you!",
                SizeToContent = SizeToContent.Manual,
                Width = 460,
                Height = 320,
            });

            toastManager.CreateToast()
                .WithTitle("Dialog Option Clicked")
                .WithContent($"You clicked option: {result}")
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        }

        [RelayCommand]
        private async Task OpenSuccessMessageBox()
        {
            var checkBox = new CheckBox()
            {
                Content = "Don't show this message again",
            };
            var result = await SukiMessageBox.ShowDialog(new SukiMessageBoxHost
            {
                IconPreset = SukiMessageBoxIcons.Success,
                ActionButtonsPreset = SukiMessageBoxButtons.OK,
                FooterLeftItemsSource = [checkBox],
                Content = "The operation was completed successfully.\nTook: 25 seconds.",
            });

            toastManager.CreateToast()
                .WithTitle("Dialog Option Clicked")
                .WithContent($"You clicked option: {result}\nDo not show again: {checkBox.IsChecked}")
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        }

        [RelayCommand]
        private async Task OpenWarningMessageBox()
        {
            var result = await SukiMessageBox.ShowDialog(new SukiMessageBoxHost
            {
                IconPreset = SukiMessageBoxIcons.Warning,
                Content = """
                      There's not internet connection to perform this action.
                      Do you want to queue a retry in 10 seconds?
                      """,
                FooterLeftItemsSource = [
                    new Border
                    {
                        Background = Brushes.Red,
                        Width = 16,
                        Height = 16,
                        CornerRadius = new CornerRadius(50),
                    },
                    new SelectableTextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = "Status: Offline",
                    },
                ],
                ActionButtonsPreset = SukiMessageBoxButtons.YesNo
            });

            toastManager.CreateToast()
                .WithTitle("Dialog Option Clicked")
                .WithContent($"You clicked option: {result}")
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        }

        [RelayCommand]
        private async Task OpenErrorMessageBox()
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    if (retryCount % 2 == 0)
                    {
                        int i = 0;
                        int error = 10 / i;
                    }
                    else
                    {
                        byte[] bytes = [];
                        var i = bytes[1];
                    }
                }
                catch (Exception e)
                {
                    var result = await SukiMessageBox.ShowDialog(new SukiMessageBoxHost
                    {
                        IconPreset = SukiMessageBoxIcons.Error,
                        Header = $"Exception: {e.GetType().Name}",
                        Content = e.ToString(),
                        FooterLeftItemsSource = [
                        SukiMessageBoxButtonsFactory.CreateButton("Copy details"),
                            new SelectableTextBlock()
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                Text = $"Retry count: {retryCount}",
                            },
                        ],
                        ActionButtonsPreset = SukiMessageBoxButtons.RetryIgnoreAbort
                    });

                    toastManager.CreateToast()
                        .WithTitle("Dialog Option Clicked")
                        .WithContent($"You clicked option: {result}")
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();

                    if (result is not SukiMessageBoxResult.Retry) break;
                    retryCount++;
                }
            }
        }

        [RelayCommand]
        private async Task OpenLongMessageBox(bool useNativeWindow = false)
        {
            var text = """"
                   ====================================================================================================
                   EXTREMELY LONG RAW STRING LITERAL DEMONSTRATION
                   ====================================================================================================
                   This C# 11+ raw string literal showcases both extensive vertical length (many lines) and substantial
                   horizontal width (long lines). The triple-quote syntax (""") allows for clean representation of complex
                   multi - line content without escape sequences while preserving all formatting exactly as written.

                   SECTION 1: VERTICAL EXPANSION(MANY LINES)
                   ----------------------------------------------------------------------------------------------------
                   Line 001: This is the first of many lines that demonstrate vertical expansion in a raw string literal.
                   Line 002: Each line is preserved exactly as written, with all whitespace maintained faithfully.
                   Line 003: No need for string concatenation or environment.newline - just type your content naturally.
                   Line 004: The compiler handles all the line breaks and formatting exactly as you see it in the code.
                   Line 005: This makes maintaining long text blocks much easier than traditional string approaches.
                   Line 006: Imagine documenting an entire API specification directly in your code as a string constant.
                   Line 007: Or embedding complete HTML templates with perfect formatting preservation.
                   Line 008: The possibilities are endless for documentation, templates, or data storage in code.
                   Line 009: Raw string literals eliminate the pain of escaped quotes and special characters.
                   Line 010: They're particularly valuable for regular expressions with many backslashes.
                   Line 011: Writing file paths becomes trivial - no more doubling up backslashes.
                   Line 012: JSON, XML, HTML, and other markup languages can be embedded cleanly.
                   Line 013: SQL queries with complex formatting maintain their readability.
                   Line 014: Multi - language support becomes simpler with preserved special characters.
                   Line 015: The text editor scrollbar is getting smaller as we add more lines.
                   Line 016: Yet the code remains perfectly readable and maintainable.
                   Line 017: Each new line adds to the vertical expansion demonstration.
                   Line 018: Notice how the closing delimiter determines indentation handling.
                   Line 019: Common indentation is automatically removed from all lines.
                   Line 020: But relative indentation within the content is preserved.

                   SECTION 2: HORIZONTAL EXPANSION(WIDE CONTENT)
                   ----------------------------------------------------------------------------------------------------
                   This portion demonstrates extremely long lines that extend far to the right, pushing the boundaries of horizontal space in the raw string literal.The line wraps in your editor, but in the actual string, it's preserved as a single, continuous line without breaks (unless explicitly included). Imagine embedding a minified JavaScript file or a very long base64-encoded string - this is where raw string literals shine for horizontal content.

                   EXAMPLE OF EXTREMELY LONG LINE: Lorem ipsum dolor sit amet, consectetur adipiscing elit.Nullam auctor, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl.Nullam auctor, nisl eget ultricies tincidunt, nisl nisl aliquam nisl, eget ultricies nisl nisl eget nisl.This line continues with more nonsense text to demonstrate horizontal expansion in raw string literals.The quick brown fox jumps over the lazy dog.Pack my box with five dozen liquor jugs.How vexingly quick daft zebras jump!

                   SECTION 3: COMBINED EXPANSION(BOTH DIRECTIONS)
                   ----------------------------------------------------------------------------------------------------
                   Line 001: Now we combine both approaches with many lines that each contain substantial horizontal content.
                   Line 002: This line contains a long horizontal expansion: The art of raw string literals lies in their ability to handle both dimensions of text expansion gracefully, without compromising readability in the source code or requiring awkward escape sequences that plague traditional string representations.
                   Line 003: Another wide line: In C# 11 and later, raw string literals provide a powerful tool for embedding complex text content directly in your code while maintaining perfect fidelity to the original formatting and structure, whether that content spans vertically, horizontally, or both.
                   Line 004: Wide content: When generating code or text programmatically, raw string literals can serve as perfect templates, containing placeholders that can be replaced at runtime while keeping all the surrounding formatting intact and perfectly readable in the source code.
                   Line 005: Both dimensions: The vertical expansion allows for clear separation of logical sections, while horizontal expansion accommodates content that needs to remain unbroken, such as long URLs, cryptographic keys, or minified resources.

                   SECTION 4: PRACTICAL APPLICATIONS
                   ----------------------------------------------------------------------------------------------------
                   1.EMBEDDED DOCUMENTATION:
                       /// This entire documentation block could be stored in a raw string literal
                       /// with perfect formatting preserved, including all the indentation and
                       /// special characters that would normally require escaping.

                       2.COMPLEX REGULAR EXPRESSIONS:
                      @"^(([^<>()[\]\\.,;:\s@""]+(\.[^<>()[\]\\.,;:\s@""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$"
                      (The above complex email regex is much more readable in a raw string literal)

                   3.DATA TEMPLATES:
                       {
                           "templateName": "Invoice",
                          "version": "1.2.3",
                          "fields": [
                              { "name": "invoiceNumber", "type": "string", "required": true},
                              { "name": "customerName", "type": "string", "required": true},
                              { "name": "items", "type": "array", "required": true}
                          ]
                      }

                       4.MULTI - LANGUAGE CONTENT:
                       Bonjour! Это тестовое сообщение.こんにちは！This shows how raw strings handle Unicode.

                    SECTION 5: CONCLUSION
                    ----------------------------------------------------------------------------------------------------
                   This massive raw string literal demonstrates both vertical and horizontal expansion capabilities in C#.
                   The content can grow in either direction without compromising readability or requiring escape sequences.
                   Raw string literals are particularly valuable for:
                   -Embedded resources and templates
                   - Complex regular expressions
                   - Multi - line documentation
                   - Preserving formatting in code generation
                   - Internationalization content
                   - Any scenario where text fidelity matters

                   ====================================================================================================
                   END OF MASSIVE RAW STRING LITERAL DEMONSTRATION
                   ====================================================================================================
                   """";
            var result = await SukiMessageBox.ShowDialog(new SukiMessageBoxHost
            {
                Content = text,
                FooterLeftItemsSource = [
                    new SelectableTextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeight.Bold,
                        Text = $"Text length: {text.Length:N}",
                    },
                ],
                ActionButtonsPreset = SukiMessageBoxButtons.OK
            }, new SukiMessageBoxOptions
            {
                UseNativeWindow = useNativeWindow
            });

            toastManager.CreateToast()
                .WithTitle("Dialog Option Clicked")
                .WithContent($"You clicked option: {result}")
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        }

        [RelayCommand]
        private async Task OpenViewModelMessageBox()
        {
            var result = await SukiMessageBox.ShowDialog(new SukiMessageBoxHost
            {
                Content = new ButtonsViewModel(),
            });

            toastManager.CreateToast()
                .WithTitle("Dialog Option Clicked")
                .WithContent($"You clicked option: {result}")
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        }
    }
}