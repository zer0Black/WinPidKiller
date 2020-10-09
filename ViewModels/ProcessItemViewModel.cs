using System;
using System.Collections.Generic;
using System.Text;
using WinPidKiller.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace WinPidKiller.ViewModels
{
    class ProcessItemViewModel : BindableBase
    {
        public ProcessInfo ProcessInfo { get; set; }

        private Boolean selectItem;
        public Boolean SelectItem
        {
            get { return selectItem; }
            set
            {
                selectItem = value;
                SetProperty(ref selectItem, value);
            }
        }
    }
}
