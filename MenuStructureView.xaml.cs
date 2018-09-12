using System;
using Apollo.Infrastructure.Controls;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;

namespace Apollo.Culinary.Views
{
    public partial class MenuStructureView
    {
        public MenuStructureView(MenuStructureViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }

        void OnDragRecordOver(object sender, DragRecordOverEventArgs e)
        {            
            object data = e.Data.GetData(typeof(RecordDragDropData));
            var source = ((RecordDragDropData)data).Records[0] as MenuStructureProxy;
            var target = e.TargetRecord as MenuStructureProxy;

            if ((target.IsRoot && (source == null || source.IsMenu)) ||
                (target.IsMenu && (source == null || source.IsMenu)))
            {            
                e.Effects = System.Windows.DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void OnStartRecordDrag(object sender, StartRecordDragEventArgs e)
        {
            var item = e.Records[0] as MenuStructureProxy;
            if(item.IsRoot)
                e.AllowDrag = false;
            else
                e.AllowDrag = true;

            e.Handled = true;
        }

        async void OnCompleteRecordDragDrop(object sender, CompleteRecordDragDropEventArgs e)
        {
            var item = e.Records[0] as MenuStructureProxy;
            var vm = DataContext as MenuStructureViewModel;
            await vm.ChangeParent(item);
        }
    }
}