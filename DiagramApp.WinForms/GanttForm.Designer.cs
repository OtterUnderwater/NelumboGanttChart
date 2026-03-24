using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiagramApp.WinForms
{
    partial class GanttForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // Базовые настройки формы
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Text = "Диаграмма Ганта";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.MinimumSize = new System.Drawing.Size(800, 600);

            // Настройка имени
            this.Name = "GanttForm";
        }
    }
}