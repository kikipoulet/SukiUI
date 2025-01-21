using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary.DockControls
{
    public class DockFactory(object context) : Factory
    {
        private readonly object _context = context;
        private IRootDock? _rootDock;
        private IDocumentDock? _documentDock;

        public override IRootDock CreateLayout()
        {
            var programDocument = new DocumentTextViewModel()
                { Id = "DocumentText", Title = "Program.cs" };
            var appDocument = new DocumentTextViewModel()
                { Id = "DocumentText", Title = "App.axaml" };

            var errorListTool = new ErrorListViewModel()
                { Id = "ErrorListTool", Title = "Error List" };
            var outputTool = new OutputViewModel()
                { Id = "OutputTool", Title = "Output" };
            var propertiesTool = new PropertiesViewModel() { Id = "PropertiesTool", Title = "Properties" };
            var solutionExploreTool = new SolutionExploreViewModel() { Id = "SolutionExploreTool", Title = "Solution Explore" };

            var leftDock = new ProportionalDock
            {
                Proportion = 0.25,
                Orientation = Orientation.Vertical,
                ActiveDockable = null,
                VisibleDockables = CreateList<IDockable>
                (
                    new ToolDock
                    {
                        ActiveDockable = solutionExploreTool,
                        VisibleDockables = CreateList<IDockable>(solutionExploreTool),
                        Alignment = Alignment.Left
                    }
                )
            };
            
            var documentDock = new DocumentDock
            {
                Proportion = 0.7,
                IsCollapsable = false,
                ActiveDockable = programDocument,
                VisibleDockables = CreateList<IDockable>(programDocument, appDocument),
                CanCreateDocument = true
            };
            
            var rightTopDock = new ProportionalDock
            {
                Proportion = 0.70,
                Orientation = Orientation.Horizontal,
                ActiveDockable = null,
                VisibleDockables = CreateList<IDockable>
                (
                    documentDock,
                    new ToolDock
                    {
                        ActiveDockable = propertiesTool,
                        VisibleDockables = CreateList<IDockable>(propertiesTool),
                        Alignment = Alignment.Top,
                    }
                )
            };
            
            var rightBottomDock = new ProportionalDock
            {
                Proportion = 0.30,
                Orientation = Orientation.Horizontal,
                ActiveDockable = null,
                VisibleDockables = CreateList<IDockable>
                (
                    new ToolDock
                    {
                        ActiveDockable = outputTool,
                        VisibleDockables = CreateList<IDockable>(errorListTool, outputTool),
                        Alignment = Alignment.Top,
                    }
                )
            };

            var rightDock = new ProportionalDock
            {
                Proportion = 0.75,
                Orientation = Orientation.Vertical,
                ActiveDockable = null,
                VisibleDockables = CreateList<IDockable>
                (
                    rightTopDock,
                   
                    rightBottomDock
                )
            };
            
            var mainLayout = new ProportionalDock
            {
                Orientation = Orientation.Horizontal,
                VisibleDockables = CreateList<IDockable>
                (
                    leftDock,
                    new ProportionalDockSplitter(),
                    rightDock
                )
            };

            var homeView = new HomeViewModel
            {
                Id = "Home",
                Title = "Home",
                ActiveDockable = mainLayout,
                VisibleDockables = CreateList<IDockable>(mainLayout)
            };

            var rootDock = CreateRootDock();

            rootDock.IsCollapsable = false;
            rootDock.ActiveDockable = homeView;
            rootDock.DefaultDockable = homeView;
            rootDock.VisibleDockables = CreateList<IDockable>(homeView);

            _documentDock = documentDock;
            _rootDock = rootDock;

            return rootDock;
        }

        public override IDockWindow? CreateWindowFrom(IDockable dockable)
        {
            var window = base.CreateWindowFrom(dockable);

            if (window != null)
            {
                window.Title = "Dock Avalonia Demo";
            }

            return window;
        }

        public override void InitLayout(IDockable layout)
        {
            ContextLocator = new Dictionary<string, Func<object?>>
            {
                ["Dashboard"] = () => layout,
                ["Home"] = () => _context
            };

            DockableLocator = new Dictionary<string, Func<IDockable?>>()
            {
                ["Root"] = () => _rootDock,
                ["Documents"] = () => _documentDock
            };

            HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            base.InitLayout(layout);
        }
    }
}