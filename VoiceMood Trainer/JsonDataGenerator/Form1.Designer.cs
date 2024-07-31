namespace JsonDataGenerator {
partial class Form1 {
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Button SelectFolderButton;
    private System.Windows.Forms.Button SaveJsonButton;
    private System.Windows.Forms.Label StatusLabel;
    private System.Windows.Forms.TextBox FolderPathTextBox;

    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent() {
        this.SelectFolderButton = new System.Windows.Forms.Button();
        this.SaveJsonButton = new System.Windows.Forms.Button();
        this.StatusLabel = new System.Windows.Forms.Label();
        this.FolderPathTextBox = new System.Windows.Forms.TextBox();
        this.SuspendLayout();
        //
        // SelectFolderButton
        //
        this.SelectFolderButton.Location = new System.Drawing.Point(12, 12);
        this.SelectFolderButton.Name = "SelectFolderButton";
        this.SelectFolderButton.Size = new System.Drawing.Size(150, 23);
        this.SelectFolderButton.TabIndex = 0;
        this.SelectFolderButton.Text = "Select Source Folder";
        this.SelectFolderButton.UseVisualStyleBackColor = true;
        this.SelectFolderButton.Click += new System.EventHandler(this.SelectFolderButton_Click);
        //
        // SaveJsonButton
        //
        this.SaveJsonButton.Location = new System.Drawing.Point(12, 70);
        this.SaveJsonButton.Name = "SaveJsonButton";
        this.SaveJsonButton.Size = new System.Drawing.Size(150, 23);
        this.SaveJsonButton.TabIndex = 1;
        this.SaveJsonButton.Text = "Save JSON File";
        this.SaveJsonButton.UseVisualStyleBackColor = true;
        this.SaveJsonButton.Click += new System.EventHandler(this.SaveJsonButton_Click);
        //
        // StatusLabel
        //
        this.StatusLabel.AutoSize = true;
        this.StatusLabel.Location = new System.Drawing.Point(12, 105);
        this.StatusLabel.Name = "StatusLabel";
        this.StatusLabel.Size = new System.Drawing.Size(38, 15);
        this.StatusLabel.TabIndex = 2;
        this.StatusLabel.Text = "Status:";
        //
        // FolderPathTextBox
        //
        this.FolderPathTextBox.Location = new System.Drawing.Point(168, 12);
        this.FolderPathTextBox.Name = "FolderPathTextBox";
        this.FolderPathTextBox.Size = new System.Drawing.Size(300, 23);
        this.FolderPathTextBox.TabIndex = 3;
        //
        // Form1
        //
        this.ClientSize = new System.Drawing.Size(480, 140);
        this.Controls.Add(this.FolderPathTextBox);
        this.Controls.Add(this.StatusLabel);
        this.Controls.Add(this.SaveJsonButton);
        this.Controls.Add(this.SelectFolderButton);
        this.Name = "Form1";
        this.Text = "JSON Data Generator";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
}
