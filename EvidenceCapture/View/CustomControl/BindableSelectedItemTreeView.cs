﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EvidenceCapture.View.CustomControl
{
    /// <summary>
    /// SelectedItem をバインド可能にする TreeView の拡張コントロールです。
    /// </summary>
    public class BindableSelectedItemTreeView : TreeView
    {
        //
        // Bindable Definitions
        // - - - - - - - - - - - - - - - - - - - -

        public static readonly DependencyProperty BindableSelectedItemProperty
        #region...
        = DependencyProperty.Register(nameof(BindableSelectedItem),
                    typeof(object), typeof(BindableSelectedItemTreeView), new PropertyMetadata(OnBindableSelectedItemChange));

        private static void OnBindableSelectedItemChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TreeView)
            {
                var tv = d as TreeView;
                var newItem = (TreeViewItem)tv.ItemContainerGenerator.ContainerFromItem(e.NewValue);
                if(newItem != null)
                {
                    newItem.IsSelected = true;
                }

            }
        }
        #endregion

        //
        // Properties
        // - - - - - - - - - - - - - - - - - - - -

        /// <summary>
        /// Bind 可能な SelectedItem を表し、SelectedItem を設定または取得します。
        /// </summary>
        public object BindableSelectedItem
        {
            get { return (object)this.GetValue(BindableSelectedItemProperty); }
            set {
                this.SetValue(BindableSelectedItemProperty, value);
            }
        }

        //
        // Constructors
        // - - - - - - - - - - - - - - - - - - - -

        public BindableSelectedItemTreeView()
        {
            this.SelectedItemChanged += this.OnSelectedItemChanged;
        }


        //
        // Event Handlers
        // - - - - - - - - - - - - - - - - - - - -

        protected virtual void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.SelectedItem == null)
            {
                return;
            }

            this.SetValue(BindableSelectedItemProperty, this.SelectedItem);
        }
    }
}
