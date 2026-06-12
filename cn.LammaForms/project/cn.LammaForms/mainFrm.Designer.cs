namespace cn.LammaForms
{
    partial class mainFrm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            notifyIcon = new NotifyIcon(components);
            contextMenuStrip = new ContextMenuStrip(components);
            显示主窗口ToolStripMenuItem = new ToolStripMenuItem();
            退出ToolStripMenuItem = new ToolStripMenuItem();
            panel_left = new Panel();
            panel_left_center = new Panel();
            gp_model_options = new GroupBox();
            panel_left_bottom = new Panel();
            splitContainer_left = new SplitContainer();
            splitContainer_main = new SplitContainer();
            gp_left_config = new GroupBox();
            button_import_options = new Button();
            button_save_options = new Button();
            btn_analyze_options = new Button();
            btn_CleanLogs = new Button();
            btn_SaveBatchCmd = new Button();
            btn_chatTest = new Button();
            btn_getOptions = new Button();
            tb_box_webConfig = new TextBox();
            panel_left_top = new Panel();
            gp_models = new GroupBox();
            btn_ggufModel_Refresh = new Button();
            cb_ggufModel = new ComboBox();
            btn_mmprojFile = new Button();
            btn_ggufModelsPath = new Button();
            btn_llamaPath = new Button();
            tb_mmprojFile = new TextBox();
            tb_mtpFile = new TextBox();
            btn_mtpFile = new Button();
            label_mtpFile = new Label();
            tb_ggufModelsPath = new TextBox();
            tb_llamaPath = new TextBox();
            label_ggufModel = new Label();
            label_mmprojFile = new Label();
            label_llamaPath = new Label();
            panel_right = new Panel();
            panel_right__logs = new Panel();
            tabControl_logs = new TabControl();
            tabPage_log_runLogs = new TabPage();
            tb_TotalInfoLogs = new TextBox();
            tabPage_log_testLogs = new TabPage();
            tb_runningLogs = new TextBox();
            tabPage_log_serverLogs = new TabPage();
            tb_serverLogs = new TextBox();
            panel_right_status = new Panel();
            tb_tokens = new TextBox();
            panel_right_top = new Panel();
            splitContainer = new SplitContainer();
            tabControl_test = new TabControl();
            tab_test_startConfig = new TabPage();
            panel_status = new Panel();
            cb_startConfig_cors = new CheckBox();
            cb_startConfig_local = new CheckBox();
            btn_openclaw_web = new Button();
            btn_modelID = new Button();
            btn_apiKey = new Button();
            btn_apiUrl = new Button();
            tb_modelID = new TextBox();
            tb_apiKey = new TextBox();
            tb_apiUrl_address = new TextBox();
            tb_apiUrl_port = new TextBox();
            tb_apiUrl = new TextBox();
            label8 = new Label();
            label3 = new Label();
            label2 = new Label();
            label6 = new Label();
            label1 = new Label();
            label5 = new Label();
            tab_test_contentBatch = new TabPage();
            btn_contentTest_Run = new Button();
            btn_contentTest_selectOther = new Button();
            btn_contentTest_selectAll = new Button();
            gp_content = new GroupBox();
            cb_contentTest_5M = new CheckBox();
            cb_contentTest_2M = new CheckBox();
            cb_contentTest_1M = new CheckBox();
            cb_contentTest_960k = new CheckBox();
            cb_contentTest_928k = new CheckBox();
            cb_contentTest_896k = new CheckBox();
            cb_contentTest_864k = new CheckBox();
            cb_contentTest_832k = new CheckBox();
            cb_contentTest_800k = new CheckBox();
            cb_contentTest_768k = new CheckBox();
            cb_contentTest_736k = new CheckBox();
            cb_contentTest_704k = new CheckBox();
            cb_contentTest_672k = new CheckBox();
            cb_contentTest_640k = new CheckBox();
            cb_contentTest_608k = new CheckBox();
            cb_contentTest_576k = new CheckBox();
            cb_contentTest_544k = new CheckBox();
            cb_contentTest_512k = new CheckBox();
            cb_contentTest_480k = new CheckBox();
            cb_contentTest_448k = new CheckBox();
            cb_contentTest_416k = new CheckBox();
            cb_contentTest_384k = new CheckBox();
            cb_contentTest_352k = new CheckBox();
            cb_contentTest_320k = new CheckBox();
            cb_contentTest_288k = new CheckBox();
            cb_contentTest_256k = new CheckBox();
            cb_contentTest_224k = new CheckBox();
            cb_contentTest_192k = new CheckBox();
            cb_contentTest_160k = new CheckBox();
            cb_contentTest_128k = new CheckBox();
            cb_contentTest_96k = new CheckBox();
            cb_contentTest_64k = new CheckBox();
            cb_contentTest_32k = new CheckBox();
            cb_contentTest_16k = new CheckBox();
            cb_contentTest_8k = new CheckBox();
            cb_contentTest_4k = new CheckBox();
            tab_test_multithreading = new TabPage();
            tb_multithreadTest_request = new Button();
            btn_multithreadTest_cpu = new Button();
            groupBox3 = new GroupBox();
            tb_multithreadTest_step = new TextBox();
            tb_multithreadTest_end = new TextBox();
            tb_multithreadTest_start = new TextBox();
            label12 = new Label();
            label13 = new Label();
            label11 = new Label();
            label10 = new Label();
            cb_multithreadTest_contentLength = new ComboBox();
            toolTip = new ToolTip(components);
            panel_left.SuspendLayout();
            panel_left_center.SuspendLayout();
            panel_left_bottom.SuspendLayout();
            gp_left_config.SuspendLayout();
            panel_left_top.SuspendLayout();
            gp_models.SuspendLayout();
            panel_right.SuspendLayout();
            panel_right__logs.SuspendLayout();
            tabControl_logs.SuspendLayout();
            tabPage_log_runLogs.SuspendLayout();
            tabPage_log_testLogs.SuspendLayout();
            tabPage_log_serverLogs.SuspendLayout();
            panel_right_status.SuspendLayout();
            panel_right_top.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer)).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            tabControl_test.SuspendLayout();
            tab_test_startConfig.SuspendLayout();
            tab_test_contentBatch.SuspendLayout();
            gp_content.SuspendLayout();
            tab_test_multithreading.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer_main)).BeginInit();
            splitContainer_main.Panel1.SuspendLayout();
            splitContainer_main.Panel2.SuspendLayout();
            splitContainer_main.SuspendLayout();
            SuspendLayout();
            // 
            // panel_left
            // 
            panel_left.Controls.Add(splitContainer_left);
            panel_left.Controls.Add(panel_left_top);
            panel_left.Dock = DockStyle.Fill;
            panel_left.Location = new Point(0, 0);
            panel_left.Name = "panel_left";
            panel_left.Size = new Size(684, 1222);
            panel_left.TabIndex = 0;
            // 
            // splitContainer_main
            // 
            splitContainer_main.Dock = DockStyle.Fill;
            splitContainer_main.Location = new Point(0, 0);
            splitContainer_main.Name = "splitContainer_main";
            splitContainer_main.Orientation = Orientation.Vertical;
            splitContainer_main.Size = new Size(1878, 1222);
            splitContainer_main.SplitterDistance = 684;
            splitContainer_main.SplitterWidth = 6;
            splitContainer_main.SplitterIncrement = 10;
            splitContainer_main.Panel1.Controls.Add(panel_left);
            splitContainer_main.Panel2.Controls.Add(panel_right);
            // 
            // splitContainer_left
            // 
            splitContainer_left.Dock = DockStyle.Fill;
            splitContainer_left.Location = new Point(0, 350);
            splitContainer_left.Name = "splitContainer_left";
            splitContainer_left.Orientation = Orientation.Horizontal;
            splitContainer_left.Size = new Size(684, 872);
            splitContainer_left.SplitterDistance = 537;
            splitContainer_left.SplitterWidth = 6;
            splitContainer_left.SplitterIncrement = 10;
            // 
            // splitContainer_left.Panel1
            // 
            splitContainer_left.Panel1.Controls.Add(panel_left_center);
            splitContainer_left.Panel1MinSize = 100;
            // 
            // splitContainer_left.Panel2
            // 
            splitContainer_left.Panel2.Controls.Add(panel_left_bottom);
            splitContainer_left.Panel2MinSize = 100;
            // 
            // panel_left_center
            // 
            panel_left_center.AutoScroll = true;
            panel_left_center.Controls.Add(gp_model_options);
            panel_left_center.Dock = DockStyle.Fill;
            panel_left_center.Location = new Point(0, 350);
            panel_left_center.Name = "panel_left_center";
            panel_left_center.Size = new Size(684, 605);
            panel_left_center.TabIndex = 2;
            // 
            // gp_model_options
            // 
            gp_model_options.Dock = DockStyle.Fill;
            gp_model_options.ForeColor = SystemColors.ActiveCaptionText;
            gp_model_options.Location = new Point(0, 0);
            gp_model_options.Name = "gp_model_options";
            gp_model_options.Size = new Size(684, 605);
            gp_model_options.TabIndex = 0;
            gp_model_options.TabStop = false;
            gp_model_options.Text = "模型参数设置";
            // 
            // panel_left_bottom
            // 
            panel_left_bottom.Controls.Add(gp_left_config);
            panel_left_bottom.Dock = DockStyle.Bottom;
            panel_left_bottom.Location = new Point(0, 887);
            panel_left_bottom.Name = "panel_left_bottom";
            panel_left_bottom.Size = new Size(684, 335);
            panel_left_bottom.TabIndex = 1;
            // 
            // gp_left_config
            // 
            gp_left_config.Controls.Add(button_import_options);
            gp_left_config.Controls.Add(button_save_options);
            gp_left_config.Controls.Add(btn_analyze_options);
            gp_left_config.Controls.Add(btn_CleanLogs);
            gp_left_config.Controls.Add(btn_SaveBatchCmd);
            gp_left_config.Controls.Add(btn_chatTest);
            gp_left_config.Controls.Add(btn_getOptions);
            gp_left_config.Controls.Add(tb_box_webConfig);
            gp_left_config.Dock = DockStyle.Fill;
            gp_left_config.Location = new Point(0, 0);
            gp_left_config.Name = "gp_left_config";
            gp_left_config.Size = new Size(684, 335);
            gp_left_config.TabIndex = 0;
            gp_left_config.TabStop = false;
            gp_left_config.Text = "启动参数配置信息";
            // 
            // button_import_options
            // 
            button_import_options.Location = new Point(517, 202);
            button_import_options.Name = "button_import_options";
            button_import_options.Size = new Size(135, 34);
            button_import_options.TabIndex = 3;
            button_import_options.Text = "引入启动参数";
            button_import_options.UseVisualStyleBackColor = true;
            button_import_options.Click += button_import_options_Click;
            // 
            // button_save_options
            // 
            button_save_options.Location = new Point(517, 242);
            button_save_options.Name = "button_save_options";
            button_save_options.Size = new Size(135, 34);
            button_save_options.TabIndex = 2;
            button_save_options.Text = "保存启动参数";
            button_save_options.UseVisualStyleBackColor = true;
            button_save_options.Click += button_save_options_Click;
            // 
            // btn_analyze_options
            // 
            btn_analyze_options.Location = new Point(498, 289);
            btn_analyze_options.Name = "btn_analyze_options";
            btn_analyze_options.Size = new Size(180, 34);
            btn_analyze_options.TabIndex = 1;
            btn_analyze_options.Text = "动态解析启动参数";
            btn_analyze_options.UseVisualStyleBackColor = true;
            btn_analyze_options.Click += btn_analyze_options_Click;
            // 
            // btn_CleanLogs
            // 
            btn_CleanLogs.Location = new Point(498, 154);
            btn_CleanLogs.Name = "btn_CleanLogs";
            btn_CleanLogs.Size = new Size(180, 34);
            btn_CleanLogs.TabIndex = 1;
            btn_CleanLogs.Text = "清空日志信息";
            btn_CleanLogs.UseVisualStyleBackColor = true;
            btn_CleanLogs.Click += btn_CleanLogs_Click;
            // 
            // btn_SaveBatchCmd
            // 
            btn_SaveBatchCmd.Location = new Point(498, 114);
            btn_SaveBatchCmd.Name = "btn_SaveBatchCmd";
            btn_SaveBatchCmd.Size = new Size(180, 34);
            btn_SaveBatchCmd.TabIndex = 1;
            btn_SaveBatchCmd.Text = "生成server启动脚本";
            btn_SaveBatchCmd.UseVisualStyleBackColor = true;
            btn_SaveBatchCmd.Click += btn_SaveBatchCmd_Click;
            // 
            // btn_chatTest
            // 
            btn_chatTest.Location = new Point(498, 74);
            btn_chatTest.Name = "btn_chatTest";
            btn_chatTest.Size = new Size(180, 34);
            btn_chatTest.TabIndex = 1;
            btn_chatTest.Text = "cmd窗口对话测试";
            btn_chatTest.UseVisualStyleBackColor = true;
            btn_chatTest.Click += btn_chatTest_Click;
            // 
            // btn_getOptions
            // 
            btn_getOptions.Location = new Point(498, 34);
            btn_getOptions.Name = "btn_getOptions";
            btn_getOptions.Size = new Size(180, 34);
            btn_getOptions.TabIndex = 1;
            btn_getOptions.Text = "获取最优启动参数";
            btn_getOptions.UseVisualStyleBackColor = true;
            btn_getOptions.Click += btn_getOptions_Click;
            // 
            // tb_box_webConfig
            // 
            tb_box_webConfig.Dock = DockStyle.Left;
            tb_box_webConfig.Location = new Point(3, 26);
            tb_box_webConfig.Multiline = true;
            tb_box_webConfig.Name = "tb_box_webConfig";
            tb_box_webConfig.ScrollBars = ScrollBars.Vertical;
            tb_box_webConfig.Size = new Size(489, 306);
            tb_box_webConfig.TabIndex = 0;
            // 
            // panel_left_top
            // 
            panel_left_top.Controls.Add(gp_models);
            panel_left_top.Dock = DockStyle.Top;
            panel_left_top.Location = new Point(0, 0);
            panel_left_top.Name = "panel_left_top";
            panel_left_top.Size = new Size(684, 350);
            panel_left_top.TabIndex = 0;
            // 
            // gp_models
            // 
            gp_models.Controls.Add(btn_ggufModel_Refresh);
            gp_models.Controls.Add(cb_ggufModel);
            gp_models.Controls.Add(btn_mmprojFile);
            gp_models.Controls.Add(btn_mtpFile);
            gp_models.Controls.Add(btn_ggufModelsPath);
            gp_models.Controls.Add(btn_llamaPath);
            gp_models.Controls.Add(tb_mtpFile);
            gp_models.Controls.Add(tb_mmprojFile);
            gp_models.Controls.Add(tb_ggufModelsPath);
            gp_models.Controls.Add(tb_llamaPath);
            gp_models.Controls.Add(label_ggufModel);
            gp_models.Controls.Add(label_mmprojFile);
            gp_models.Controls.Add(label_mtpFile);
            gp_models.Controls.Add(label_llamaPath);
            gp_models.Dock = DockStyle.Fill;
            gp_models.Location = new Point(0, 0);
            gp_models.Name = "gp_models";
            gp_models.Size = new Size(684, 282);
            gp_models.TabIndex = 0;
            gp_models.TabStop = false;
            gp_models.Text = "模型选择";
            // 
            // btn_ggufModel_Refresh
            // 
            btn_ggufModel_Refresh.Location = new Point(593, 165);
            btn_ggufModel_Refresh.Name = "btn_ggufModel_Refresh";
            btn_ggufModel_Refresh.Size = new Size(85, 34);
            btn_ggufModel_Refresh.TabIndex = 4;
            btn_ggufModel_Refresh.Text = "刷 新";
            btn_ggufModel_Refresh.UseVisualStyleBackColor = true;
            btn_ggufModel_Refresh.Click += btn_ggufModel_Refresh_Click;
            // 
            // cb_ggufModel
            // 
            cb_ggufModel.FormattingEnabled = true;
            cb_ggufModel.Location = new Point(24, 164);
            cb_ggufModel.Name = "cb_ggufModel";
            cb_ggufModel.Size = new Size(563, 32);
            cb_ggufModel.TabIndex = 3;
            // 
            // btn_mmprojFile
            // 
            btn_mmprojFile.Location = new Point(593, 235);
            btn_mmprojFile.Name = "btn_mmprojFile";
            btn_mmprojFile.Size = new Size(85, 34);
            btn_mmprojFile.TabIndex = 2;
            btn_mmprojFile.Text = "浏览...";
            btn_mmprojFile.UseVisualStyleBackColor = true;
            btn_mmprojFile.Click += btn_mmprojFile_Click;
            // 
            // btn_ggufModelsPath
            // 
            btn_ggufModelsPath.Location = new Point(593, 119);
            btn_ggufModelsPath.Name = "btn_ggufModelsPath";
            btn_ggufModelsPath.Size = new Size(85, 34);
            btn_ggufModelsPath.TabIndex = 2;
            btn_ggufModelsPath.Text = "浏览...";
            btn_ggufModelsPath.UseVisualStyleBackColor = true;
            btn_ggufModelsPath.Click += btn_ggufModelsPath_Click;
            // 
            // btn_llamaPath
            // 
            btn_llamaPath.Location = new Point(593, 53);
            btn_llamaPath.Name = "btn_llamaPath";
            btn_llamaPath.Size = new Size(85, 34);
            btn_llamaPath.TabIndex = 2;
            btn_llamaPath.Text = "浏览...";
            btn_llamaPath.UseVisualStyleBackColor = true;
            btn_llamaPath.Click += btn_llamaPath_Click;
            // 
            // tb_mmprojFile
            // 
            tb_mmprojFile.Location = new Point(24, 237);
            tb_mmprojFile.Name = "tb_mmprojFile";
            tb_mmprojFile.Size = new Size(563, 30);
            tb_mmprojFile.TabIndex = 1;
            // 
            // tb_ggufModelsPath
            // 
            tb_ggufModelsPath.Location = new Point(24, 119);
            tb_ggufModelsPath.Name = "tb_ggufModelsPath";
            tb_ggufModelsPath.Size = new Size(563, 30);
            tb_ggufModelsPath.TabIndex = 1;
            // 
            // tb_llamaPath
            // 
            tb_llamaPath.Location = new Point(24, 53);
            tb_llamaPath.Name = "tb_llamaPath";
            tb_llamaPath.Size = new Size(563, 30);
            tb_llamaPath.TabIndex = 1;
            // 
            // label_ggufModel
            // 
            label_ggufModel.AutoSize = true;
            label_ggufModel.Location = new Point(17, 90);
            label_ggufModel.Name = "label_ggufModel";
            label_ggufModel.Size = new Size(413, 24);
            label_ggufModel.TabIndex = 0;
            label_ggufModel.Text = "GGUF模型文件夹(支持多个模型、系统自动查找)：";
            // 
            // label_mmprojFile
            // 
            label_mmprojFile.AutoSize = true;
            label_mmprojFile.Location = new Point(17, 208);
            label_mmprojFile.Name = "label_mmprojFile";
            label_mmprojFile.Size = new Size(190, 24);
            label_mmprojFile.TabIndex = 0;
            label_mmprojFile.Text = "多模态投影模型文件：";
            // 
            // label_mtpFile
            // 
            label_mtpFile.AutoSize = true;
            label_mtpFile.Location = new Point(17, 278);
            label_mtpFile.Name = "label_mtpFile";
            label_mtpFile.Size = new Size(160, 24);
            label_mtpFile.TabIndex = 0;
            label_mtpFile.Text = "MTP 模型文件：";
            // 
            // tb_mtpFile
            // 
            tb_mtpFile.Location = new Point(24, 307);
            tb_mtpFile.Name = "tb_mtpFile";
            tb_mtpFile.Size = new Size(563, 30);
            tb_mtpFile.TabIndex = 5;
            // 
            // btn_mtpFile
            // 
            btn_mtpFile.Location = new Point(593, 305);
            btn_mtpFile.Name = "btn_mtpFile";
            btn_mtpFile.Size = new Size(85, 34);
            btn_mtpFile.TabIndex = 6;
            btn_mtpFile.Text = "浏览...";
            btn_mtpFile.UseVisualStyleBackColor = true;
            btn_mtpFile.Click += btn_mtpFile_Click;
            // 
            // label_llamaPath
            // 
            label_llamaPath.AutoSize = true;
            label_llamaPath.Location = new Point(17, 26);
            label_llamaPath.Name = "label_llamaPath";
            label_llamaPath.Size = new Size(200, 24);
            label_llamaPath.TabIndex = 0;
            label_llamaPath.Text = "Lamma.cpp工具地址：";
            // 
            // panel_right
            // 
            panel_right.Controls.Add(splitContainer);
            panel_right.Controls.Add(panel_right_status);
            panel_right.Dock = DockStyle.Fill;
            panel_right.Name = "panel_right";
            panel_right.Size = new Size(1194, 1222);
            panel_right.TabIndex = 1;
            // 
            // panel_right__logs
            // 
            panel_right__logs.Controls.Add(tabControl_logs);
            panel_right__logs.Dock = DockStyle.Fill;
            panel_right__logs.Location = new Point(0, 340);
            panel_right__logs.Name = "panel_right__logs";
            panel_right__logs.Size = new Size(1194, 882);
            panel_right__logs.TabIndex = 2;
            // 
            // tabControl_logs
            // 
            tabControl_logs.Controls.Add(tabPage_log_runLogs);
            tabControl_logs.Controls.Add(tabPage_log_testLogs);
            tabControl_logs.Controls.Add(tabPage_log_serverLogs);
            tabControl_logs.Dock = DockStyle.Fill;
            tabControl_logs.Location = new Point(0, 0);
            tabControl_logs.Name = "tabControl_logs";
            tabControl_logs.SelectedIndex = 0;
            tabControl_logs.Size = new Size(1194, 882);
            tabControl_logs.TabIndex = 0;
            // 
            // tabPage_log_runLogs
            // 
            tabPage_log_runLogs.Controls.Add(tb_TotalInfoLogs);
            tabPage_log_runLogs.Location = new Point(4, 33);
            tabPage_log_runLogs.Name = "tabPage_log_runLogs";
            tabPage_log_runLogs.Padding = new Padding(3);
            tabPage_log_runLogs.Size = new Size(1186, 845);
            tabPage_log_runLogs.TabIndex = 0;
            tabPage_log_runLogs.Text = "运行日志";
            tabPage_log_runLogs.UseVisualStyleBackColor = true;
            // 
            // tb_TotalInfoLogs
            // 
            tb_TotalInfoLogs.Dock = DockStyle.Fill;
            tb_TotalInfoLogs.Location = new Point(3, 3);
            tb_TotalInfoLogs.Multiline = true;
            tb_TotalInfoLogs.Name = "tb_TotalInfoLogs";
            tb_TotalInfoLogs.ScrollBars = ScrollBars.Vertical;
            tb_TotalInfoLogs.Size = new Size(1180, 839);
            tb_TotalInfoLogs.TabIndex = 0;
            // 
            // tabPage_log_testLogs
            // 
            tabPage_log_testLogs.Controls.Add(tb_runningLogs);
            tabPage_log_testLogs.Location = new Point(4, 33);
            tabPage_log_testLogs.Name = "tabPage_log_testLogs";
            tabPage_log_testLogs.Padding = new Padding(3);
            tabPage_log_testLogs.Size = new Size(1186, 845);
            tabPage_log_testLogs.TabIndex = 1;
            tabPage_log_testLogs.Text = "测试结果";
            tabPage_log_testLogs.UseVisualStyleBackColor = true;
            // 
            // tb_runningLogs
            // 
            tb_runningLogs.Dock = DockStyle.Fill;
            tb_runningLogs.Location = new Point(3, 3);
            tb_runningLogs.Multiline = true;
            tb_runningLogs.Name = "tb_runningLogs";
            tb_runningLogs.ScrollBars = ScrollBars.Vertical;
            tb_runningLogs.Size = new Size(1180, 839);
            tb_runningLogs.TabIndex = 0;
            // 
            // tabPage_log_serverLogs
            // 
            tabPage_log_serverLogs.Controls.Add(tb_serverLogs);
            tabPage_log_serverLogs.Location = new Point(4, 33);
            tabPage_log_serverLogs.Name = "tabPage_log_serverLogs";
            tabPage_log_serverLogs.Size = new Size(1186, 845);
            tabPage_log_serverLogs.TabIndex = 2;
            tabPage_log_serverLogs.Text = "服务器日志";
            tabPage_log_serverLogs.UseVisualStyleBackColor = true;
            // 
            // tb_serverLogs
            // 
            tb_serverLogs.Dock = DockStyle.Fill;
            tb_serverLogs.Location = new Point(0, 0);
            tb_serverLogs.Multiline = true;
            tb_serverLogs.Name = "tb_serverLogs";
            tb_serverLogs.ScrollBars = ScrollBars.Vertical;
            tb_serverLogs.Size = new Size(1186, 845);
            tb_serverLogs.TabIndex = 0;
            // 
            // panel_right_status
            // 
            panel_right_status.Controls.Add(tb_tokens);
            panel_right_status.Dock = DockStyle.Top;
            panel_right_status.Location = new Point(0, 236);
            panel_right_status.Name = "panel_right_status";
            panel_right_status.Size = new Size(1194, 104);
            panel_right_status.TabIndex = 1;
            // 
            // tb_tokens
            // 
            tb_tokens.Dock = DockStyle.Fill;
            tb_tokens.Location = new Point(0, 0);
            tb_tokens.Multiline = true;
            tb_tokens.Name = "tb_tokens";
            tb_tokens.ScrollBars = ScrollBars.Vertical;
            tb_tokens.Size = new Size(1194, 104);
            tb_tokens.TabIndex = 0;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.Size = new Size(1194, 870);
            splitContainer.SplitterDistance = 470;
            splitContainer.SplitterWidth = 6;
            splitContainer.SplitterIncrement = 10;
            splitContainer.TabIndex = 0;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(tabControl_test);
            splitContainer.Panel1MinSize = 100;
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(panel_right__logs);
            splitContainer.Panel2MinSize = 100;
            // 
            // tabControl_test
            // 
            tabControl_test.Controls.Add(tab_test_startConfig);
            tabControl_test.Controls.Add(tab_test_contentBatch);
            tabControl_test.Controls.Add(tab_test_multithreading);
            tabControl_test.Dock = DockStyle.Fill;
            tabControl_test.Location = new Point(0, 0);
            tabControl_test.Name = "tabControl_test";
            tabControl_test.SelectedIndex = 0;
            tabControl_test.Size = new Size(1194, 470);
            tabControl_test.TabIndex = 0;
            // 
            // tab_test_startConfig
            // 
            tab_test_startConfig.Controls.Add(panel_status);
            tab_test_startConfig.Controls.Add(cb_startConfig_cors);
            tab_test_startConfig.Controls.Add(cb_startConfig_local);
            tab_test_startConfig.Controls.Add(btn_openclaw_web);
            tab_test_startConfig.Controls.Add(btn_modelID);
            tab_test_startConfig.Controls.Add(btn_apiKey);
            tab_test_startConfig.Controls.Add(btn_apiUrl);
            tab_test_startConfig.Controls.Add(tb_modelID);
            tab_test_startConfig.Controls.Add(tb_apiKey);
            tab_test_startConfig.Controls.Add(tb_apiUrl_address);
            tab_test_startConfig.Controls.Add(tb_apiUrl_port);
            tab_test_startConfig.Controls.Add(tb_apiUrl);
            tab_test_startConfig.Controls.Add(label8);
            tab_test_startConfig.Controls.Add(label3);
            tab_test_startConfig.Controls.Add(label2);
            tab_test_startConfig.Controls.Add(label6);
            tab_test_startConfig.Controls.Add(label1);
            tab_test_startConfig.Controls.Add(label5);
            tab_test_startConfig.Location = new Point(4, 33);
            tab_test_startConfig.Name = "tab_test_startConfig";
            tab_test_startConfig.Padding = new Padding(3);
            tab_test_startConfig.Size = new Size(1186, 199);
            tab_test_startConfig.TabIndex = 0;
            tab_test_startConfig.Text = "1、服务器启动参数";
            tab_test_startConfig.UseVisualStyleBackColor = true;
            // 
            // panel_status
            // 
            panel_status.Location = new Point(760, 10);
            panel_status.Name = "panel_status";
            panel_status.Size = new Size(418, 128);
            panel_status.TabIndex = 17;
            // 
            // cb_startConfig_cors
            // 
            cb_startConfig_cors.AutoSize = true;
            cb_startConfig_cors.Location = new Point(658, 11);
            cb_startConfig_cors.Name = "cb_startConfig_cors";
            cb_startConfig_cors.Size = new Size(72, 28);
            cb_startConfig_cors.TabIndex = 16;
            cb_startConfig_cors.Text = "跨域";
            cb_startConfig_cors.UseVisualStyleBackColor = true;
            cb_startConfig_cors.CheckedChanged += cb_startConfig_local_CheckedChanged;
            // 
            // cb_startConfig_local
            // 
            cb_startConfig_local.AutoSize = true;
            cb_startConfig_local.Checked = true;
            cb_startConfig_local.CheckState = CheckState.Checked;
            cb_startConfig_local.Location = new Point(265, 13);
            cb_startConfig_local.Name = "cb_startConfig_local";
            cb_startConfig_local.Size = new Size(108, 28);
            cb_startConfig_local.TabIndex = 16;
            cb_startConfig_local.Text = "本地访问";
            cb_startConfig_local.UseVisualStyleBackColor = true;
            cb_startConfig_local.CheckedChanged += cb_startConfig_local_CheckedChanged;
            // 
            // btn_openclaw_web
            // 
            btn_openclaw_web.Location = new Point(760, 144);
            btn_openclaw_web.Name = "btn_openclaw_web";
            btn_openclaw_web.Size = new Size(418, 49);
            btn_openclaw_web.TabIndex = 15;
            btn_openclaw_web.Text = "开启 llama.cpp 的web访问";
            btn_openclaw_web.UseVisualStyleBackColor = true;
            btn_openclaw_web.Click += btn_openclaw_web_Click;
            // 
            // btn_modelID
            // 
            btn_modelID.Location = new Point(672, 150);
            btn_modelID.Name = "btn_modelID";
            btn_modelID.Size = new Size(82, 34);
            btn_modelID.TabIndex = 12;
            btn_modelID.Text = "复制";
            btn_modelID.UseVisualStyleBackColor = true;
            btn_modelID.Click += btn_modelID_Click;
            // 
            // btn_apiKey
            // 
            btn_apiKey.Location = new Point(672, 102);
            btn_apiKey.Name = "btn_apiKey";
            btn_apiKey.Size = new Size(82, 34);
            btn_apiKey.TabIndex = 13;
            btn_apiKey.Text = "复制";
            btn_apiKey.UseVisualStyleBackColor = true;
            btn_apiKey.Click += btn_apiKey_Click;
            // 
            // btn_apiUrl
            // 
            btn_apiUrl.Location = new Point(672, 47);
            btn_apiUrl.Name = "btn_apiUrl";
            btn_apiUrl.Size = new Size(82, 34);
            btn_apiUrl.TabIndex = 14;
            btn_apiUrl.Text = "复制";
            btn_apiUrl.UseVisualStyleBackColor = true;
            btn_apiUrl.Click += btn_apiUrl_Click;
            // 
            // tb_modelID
            // 
            tb_modelID.Location = new Point(138, 154);
            tb_modelID.Name = "tb_modelID";
            tb_modelID.Size = new Size(518, 30);
            tb_modelID.TabIndex = 9;
            // 
            // tb_apiKey
            // 
            tb_apiKey.Location = new Point(138, 104);
            tb_apiKey.Name = "tb_apiKey";
            tb_apiKey.Size = new Size(518, 30);
            tb_apiKey.TabIndex = 10;
            tb_apiKey.Text = "sk-1234567809";
            // 
            // tb_apiUrl_address
            // 
            tb_apiUrl_address.Location = new Point(138, 10);
            tb_apiUrl_address.Name = "tb_apiUrl_address";
            tb_apiUrl_address.Size = new Size(121, 30);
            tb_apiUrl_address.TabIndex = 11;
            tb_apiUrl_address.Text = "127.0.0.1";
            // 
            // tb_apiUrl_port
            // 
            tb_apiUrl_port.Location = new Point(467, 11);
            tb_apiUrl_port.Name = "tb_apiUrl_port";
            tb_apiUrl_port.Size = new Size(75, 30);
            tb_apiUrl_port.TabIndex = 11;
            tb_apiUrl_port.Text = "8090";
            // 
            // tb_apiUrl
            // 
            tb_apiUrl.Location = new Point(138, 51);
            tb_apiUrl.Name = "tb_apiUrl";
            tb_apiUrl.ReadOnly = true;
            tb_apiUrl.Size = new Size(518, 30);
            tb_apiUrl.TabIndex = 11;
            tb_apiUrl.Text = "http://localhost:8090/v1";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(17, 158);
            label8.Name = "label8";
            label8.Size = new Size(108, 24);
            label8.TabIndex = 8;
            label8.Text = "Model ID：";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(26, 13);
            label3.Name = "label3";
            label3.Size = new Size(100, 24);
            label3.TabIndex = 6;
            label3.Text = "监听地址：";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(564, 13);
            label2.Name = "label2";
            label2.Size = new Size(102, 24);
            label2.TabIndex = 6;
            label2.Text = "Cors支持：";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(32, 106);
            label6.Name = "label6";
            label6.Size = new Size(93, 24);
            label6.TabIndex = 7;
            label6.Text = "API Key：";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(379, 14);
            label1.Name = "label1";
            label1.Size = new Size(98, 24);
            label1.TabIndex = 6;
            label1.Text = "API Port：";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(29, 54);
            label5.Name = "label5";
            label5.Size = new Size(96, 24);
            label5.TabIndex = 6;
            label5.Text = "API URL：";
            // 
            // tab_test_contentBatch
            // 
            tab_test_contentBatch.Controls.Add(btn_contentTest_Run);
            tab_test_contentBatch.Controls.Add(btn_contentTest_selectOther);
            tab_test_contentBatch.Controls.Add(btn_contentTest_selectAll);
            tab_test_contentBatch.Controls.Add(gp_content);
            tab_test_contentBatch.Location = new Point(4, 33);
            tab_test_contentBatch.Name = "tab_test_contentBatch";
            tab_test_contentBatch.Padding = new Padding(3);
            tab_test_contentBatch.Size = new Size(1186, 199);
            tab_test_contentBatch.TabIndex = 1;
            tab_test_contentBatch.Text = "2、上下文批量测试";
            tab_test_contentBatch.UseVisualStyleBackColor = true;
            // 
            // btn_contentTest_Run
            // 
            btn_contentTest_Run.Location = new Point(971, 131);
            btn_contentTest_Run.Name = "btn_contentTest_Run";
            btn_contentTest_Run.Size = new Size(140, 34);
            btn_contentTest_Run.TabIndex = 6;
            btn_contentTest_Run.Text = "开始批量测速";
            btn_contentTest_Run.UseVisualStyleBackColor = true;
            btn_contentTest_Run.Click += btn_contentTest_Run_Click;
            // 
            // btn_contentTest_selectOther
            // 
            btn_contentTest_selectOther.Location = new Point(971, 90);
            btn_contentTest_selectOther.Name = "btn_contentTest_selectOther";
            btn_contentTest_selectOther.Size = new Size(140, 34);
            btn_contentTest_selectOther.TabIndex = 5;
            btn_contentTest_selectOther.Text = "反选";
            btn_contentTest_selectOther.UseVisualStyleBackColor = true;
            btn_contentTest_selectOther.Click += btn_contentTest_selectOther_Click;
            // 
            // btn_contentTest_selectAll
            // 
            btn_contentTest_selectAll.Location = new Point(971, 48);
            btn_contentTest_selectAll.Name = "btn_contentTest_selectAll";
            btn_contentTest_selectAll.Size = new Size(140, 34);
            btn_contentTest_selectAll.TabIndex = 4;
            btn_contentTest_selectAll.Text = "全选";
            btn_contentTest_selectAll.UseVisualStyleBackColor = true;
            btn_contentTest_selectAll.Click += btn_contentTest_selectAll_Click;
            // 
            // gp_content
            // 
            gp_content.Controls.Add(cb_contentTest_5M);
            gp_content.Controls.Add(cb_contentTest_2M);
            gp_content.Controls.Add(cb_contentTest_1M);
            gp_content.Controls.Add(cb_contentTest_960k);
            gp_content.Controls.Add(cb_contentTest_928k);
            gp_content.Controls.Add(cb_contentTest_896k);
            gp_content.Controls.Add(cb_contentTest_864k);
            gp_content.Controls.Add(cb_contentTest_832k);
            gp_content.Controls.Add(cb_contentTest_800k);
            gp_content.Controls.Add(cb_contentTest_768k);
            gp_content.Controls.Add(cb_contentTest_736k);
            gp_content.Controls.Add(cb_contentTest_704k);
            gp_content.Controls.Add(cb_contentTest_672k);
            gp_content.Controls.Add(cb_contentTest_640k);
            gp_content.Controls.Add(cb_contentTest_608k);
            gp_content.Controls.Add(cb_contentTest_576k);
            gp_content.Controls.Add(cb_contentTest_544k);
            gp_content.Controls.Add(cb_contentTest_512k);
            gp_content.Controls.Add(cb_contentTest_480k);
            gp_content.Controls.Add(cb_contentTest_448k);
            gp_content.Controls.Add(cb_contentTest_416k);
            gp_content.Controls.Add(cb_contentTest_384k);
            gp_content.Controls.Add(cb_contentTest_352k);
            gp_content.Controls.Add(cb_contentTest_320k);
            gp_content.Controls.Add(cb_contentTest_288k);
            gp_content.Controls.Add(cb_contentTest_256k);
            gp_content.Controls.Add(cb_contentTest_224k);
            gp_content.Controls.Add(cb_contentTest_192k);
            gp_content.Controls.Add(cb_contentTest_160k);
            gp_content.Controls.Add(cb_contentTest_128k);
            gp_content.Controls.Add(cb_contentTest_96k);
            gp_content.Controls.Add(cb_contentTest_64k);
            gp_content.Controls.Add(cb_contentTest_32k);
            gp_content.Controls.Add(cb_contentTest_16k);
            gp_content.Controls.Add(cb_contentTest_8k);
            gp_content.Controls.Add(cb_contentTest_4k);
            gp_content.Location = new Point(6, 6);
            gp_content.Name = "gp_content";
            gp_content.Size = new Size(933, 182);
            gp_content.TabIndex = 1;
            gp_content.TabStop = false;
            gp_content.Text = "选择要测试的上下文";
            // 
            // cb_contentTest_5M
            // 
            cb_contentTest_5M.AutoSize = true;
            cb_contentTest_5M.Location = new Point(828, 141);
            cb_contentTest_5M.Name = "cb_contentTest_5M";
            cb_contentTest_5M.Size = new Size(65, 28);
            cb_contentTest_5M.TabIndex = 0;
            cb_contentTest_5M.Text = "5M";
            cb_contentTest_5M.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_2M
            // 
            cb_contentTest_2M.AutoSize = true;
            cb_contentTest_2M.Location = new Point(718, 141);
            cb_contentTest_2M.Name = "cb_contentTest_2M";
            cb_contentTest_2M.Size = new Size(65, 28);
            cb_contentTest_2M.TabIndex = 0;
            cb_contentTest_2M.Text = "2M";
            cb_contentTest_2M.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_1M
            // 
            cb_contentTest_1M.AutoSize = true;
            cb_contentTest_1M.Location = new Point(610, 141);
            cb_contentTest_1M.Name = "cb_contentTest_1M";
            cb_contentTest_1M.Size = new Size(65, 28);
            cb_contentTest_1M.TabIndex = 0;
            cb_contentTest_1M.Text = "1M";
            cb_contentTest_1M.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_960k
            // 
            cb_contentTest_960k.AutoSize = true;
            cb_contentTest_960k.Location = new Point(502, 141);
            cb_contentTest_960k.Name = "cb_contentTest_960k";
            cb_contentTest_960k.Size = new Size(80, 28);
            cb_contentTest_960k.TabIndex = 0;
            cb_contentTest_960k.Text = "960K";
            cb_contentTest_960k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_928k
            // 
            cb_contentTest_928k.AutoSize = true;
            cb_contentTest_928k.Location = new Point(394, 141);
            cb_contentTest_928k.Name = "cb_contentTest_928k";
            cb_contentTest_928k.Size = new Size(80, 28);
            cb_contentTest_928k.TabIndex = 0;
            cb_contentTest_928k.Text = "928K";
            cb_contentTest_928k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_896k
            // 
            cb_contentTest_896k.AutoSize = true;
            cb_contentTest_896k.Location = new Point(297, 141);
            cb_contentTest_896k.Name = "cb_contentTest_896k";
            cb_contentTest_896k.Size = new Size(80, 28);
            cb_contentTest_896k.TabIndex = 0;
            cb_contentTest_896k.Text = "896K";
            cb_contentTest_896k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_864k
            // 
            cb_contentTest_864k.AutoSize = true;
            cb_contentTest_864k.Location = new Point(201, 141);
            cb_contentTest_864k.Name = "cb_contentTest_864k";
            cb_contentTest_864k.Size = new Size(80, 28);
            cb_contentTest_864k.TabIndex = 0;
            cb_contentTest_864k.Text = "864K";
            cb_contentTest_864k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_832k
            // 
            cb_contentTest_832k.AutoSize = true;
            cb_contentTest_832k.Location = new Point(102, 141);
            cb_contentTest_832k.Name = "cb_contentTest_832k";
            cb_contentTest_832k.Size = new Size(80, 28);
            cb_contentTest_832k.TabIndex = 0;
            cb_contentTest_832k.Text = "832K";
            cb_contentTest_832k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_800k
            // 
            cb_contentTest_800k.AutoSize = true;
            cb_contentTest_800k.Location = new Point(7, 141);
            cb_contentTest_800k.Name = "cb_contentTest_800k";
            cb_contentTest_800k.Size = new Size(80, 28);
            cb_contentTest_800k.TabIndex = 0;
            cb_contentTest_800k.Text = "800K";
            cb_contentTest_800k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_768k
            // 
            cb_contentTest_768k.AutoSize = true;
            cb_contentTest_768k.Location = new Point(828, 101);
            cb_contentTest_768k.Name = "cb_contentTest_768k";
            cb_contentTest_768k.Size = new Size(80, 28);
            cb_contentTest_768k.TabIndex = 0;
            cb_contentTest_768k.Text = "768K";
            cb_contentTest_768k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_736k
            // 
            cb_contentTest_736k.AutoSize = true;
            cb_contentTest_736k.Location = new Point(718, 101);
            cb_contentTest_736k.Name = "cb_contentTest_736k";
            cb_contentTest_736k.Size = new Size(80, 28);
            cb_contentTest_736k.TabIndex = 0;
            cb_contentTest_736k.Text = "736K";
            cb_contentTest_736k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_704k
            // 
            cb_contentTest_704k.AutoSize = true;
            cb_contentTest_704k.Location = new Point(610, 101);
            cb_contentTest_704k.Name = "cb_contentTest_704k";
            cb_contentTest_704k.Size = new Size(80, 28);
            cb_contentTest_704k.TabIndex = 0;
            cb_contentTest_704k.Text = "704K";
            cb_contentTest_704k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_672k
            // 
            cb_contentTest_672k.AutoSize = true;
            cb_contentTest_672k.Location = new Point(502, 101);
            cb_contentTest_672k.Name = "cb_contentTest_672k";
            cb_contentTest_672k.Size = new Size(80, 28);
            cb_contentTest_672k.TabIndex = 0;
            cb_contentTest_672k.Text = "672K";
            cb_contentTest_672k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_640k
            // 
            cb_contentTest_640k.AutoSize = true;
            cb_contentTest_640k.Location = new Point(394, 101);
            cb_contentTest_640k.Name = "cb_contentTest_640k";
            cb_contentTest_640k.Size = new Size(80, 28);
            cb_contentTest_640k.TabIndex = 0;
            cb_contentTest_640k.Text = "640K";
            cb_contentTest_640k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_608k
            // 
            cb_contentTest_608k.AutoSize = true;
            cb_contentTest_608k.Location = new Point(297, 101);
            cb_contentTest_608k.Name = "cb_contentTest_608k";
            cb_contentTest_608k.Size = new Size(80, 28);
            cb_contentTest_608k.TabIndex = 0;
            cb_contentTest_608k.Text = "608K";
            cb_contentTest_608k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_576k
            // 
            cb_contentTest_576k.AutoSize = true;
            cb_contentTest_576k.Location = new Point(201, 101);
            cb_contentTest_576k.Name = "cb_contentTest_576k";
            cb_contentTest_576k.Size = new Size(80, 28);
            cb_contentTest_576k.TabIndex = 0;
            cb_contentTest_576k.Text = "576K";
            cb_contentTest_576k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_544k
            // 
            cb_contentTest_544k.AutoSize = true;
            cb_contentTest_544k.Location = new Point(102, 101);
            cb_contentTest_544k.Name = "cb_contentTest_544k";
            cb_contentTest_544k.Size = new Size(80, 28);
            cb_contentTest_544k.TabIndex = 0;
            cb_contentTest_544k.Text = "544K";
            cb_contentTest_544k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_512k
            // 
            cb_contentTest_512k.AutoSize = true;
            cb_contentTest_512k.Checked = true;
            cb_contentTest_512k.CheckState = CheckState.Checked;
            cb_contentTest_512k.Location = new Point(7, 101);
            cb_contentTest_512k.Name = "cb_contentTest_512k";
            cb_contentTest_512k.Size = new Size(80, 28);
            cb_contentTest_512k.TabIndex = 0;
            cb_contentTest_512k.Text = "512K";
            cb_contentTest_512k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_480k
            // 
            cb_contentTest_480k.AutoSize = true;
            cb_contentTest_480k.Location = new Point(828, 63);
            cb_contentTest_480k.Name = "cb_contentTest_480k";
            cb_contentTest_480k.Size = new Size(80, 28);
            cb_contentTest_480k.TabIndex = 0;
            cb_contentTest_480k.Text = "480K";
            cb_contentTest_480k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_448k
            // 
            cb_contentTest_448k.AutoSize = true;
            cb_contentTest_448k.Location = new Point(718, 63);
            cb_contentTest_448k.Name = "cb_contentTest_448k";
            cb_contentTest_448k.Size = new Size(80, 28);
            cb_contentTest_448k.TabIndex = 0;
            cb_contentTest_448k.Text = "448K";
            cb_contentTest_448k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_416k
            // 
            cb_contentTest_416k.AutoSize = true;
            cb_contentTest_416k.Location = new Point(610, 63);
            cb_contentTest_416k.Name = "cb_contentTest_416k";
            cb_contentTest_416k.Size = new Size(80, 28);
            cb_contentTest_416k.TabIndex = 0;
            cb_contentTest_416k.Text = "416K";
            cb_contentTest_416k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_384k
            // 
            cb_contentTest_384k.AutoSize = true;
            cb_contentTest_384k.Location = new Point(502, 63);
            cb_contentTest_384k.Name = "cb_contentTest_384k";
            cb_contentTest_384k.Size = new Size(80, 28);
            cb_contentTest_384k.TabIndex = 0;
            cb_contentTest_384k.Text = "384K";
            cb_contentTest_384k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_352k
            // 
            cb_contentTest_352k.AutoSize = true;
            cb_contentTest_352k.Location = new Point(394, 63);
            cb_contentTest_352k.Name = "cb_contentTest_352k";
            cb_contentTest_352k.Size = new Size(80, 28);
            cb_contentTest_352k.TabIndex = 0;
            cb_contentTest_352k.Text = "352K";
            cb_contentTest_352k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_320k
            // 
            cb_contentTest_320k.AutoSize = true;
            cb_contentTest_320k.Location = new Point(297, 63);
            cb_contentTest_320k.Name = "cb_contentTest_320k";
            cb_contentTest_320k.Size = new Size(80, 28);
            cb_contentTest_320k.TabIndex = 0;
            cb_contentTest_320k.Text = "320K";
            cb_contentTest_320k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_288k
            // 
            cb_contentTest_288k.AutoSize = true;
            cb_contentTest_288k.Location = new Point(201, 63);
            cb_contentTest_288k.Name = "cb_contentTest_288k";
            cb_contentTest_288k.Size = new Size(80, 28);
            cb_contentTest_288k.TabIndex = 0;
            cb_contentTest_288k.Text = "288K";
            cb_contentTest_288k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_256k
            // 
            cb_contentTest_256k.AutoSize = true;
            cb_contentTest_256k.Checked = true;
            cb_contentTest_256k.CheckState = CheckState.Checked;
            cb_contentTest_256k.Location = new Point(102, 63);
            cb_contentTest_256k.Name = "cb_contentTest_256k";
            cb_contentTest_256k.Size = new Size(80, 28);
            cb_contentTest_256k.TabIndex = 0;
            cb_contentTest_256k.Text = "256K";
            cb_contentTest_256k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_224k
            // 
            cb_contentTest_224k.AutoSize = true;
            cb_contentTest_224k.Location = new Point(7, 63);
            cb_contentTest_224k.Name = "cb_contentTest_224k";
            cb_contentTest_224k.Size = new Size(80, 28);
            cb_contentTest_224k.TabIndex = 0;
            cb_contentTest_224k.Text = "224K";
            cb_contentTest_224k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_192k
            // 
            cb_contentTest_192k.AutoSize = true;
            cb_contentTest_192k.Location = new Point(828, 25);
            cb_contentTest_192k.Name = "cb_contentTest_192k";
            cb_contentTest_192k.Size = new Size(80, 28);
            cb_contentTest_192k.TabIndex = 0;
            cb_contentTest_192k.Text = "192K";
            cb_contentTest_192k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_160k
            // 
            cb_contentTest_160k.AutoSize = true;
            cb_contentTest_160k.Location = new Point(718, 25);
            cb_contentTest_160k.Name = "cb_contentTest_160k";
            cb_contentTest_160k.Size = new Size(80, 28);
            cb_contentTest_160k.TabIndex = 0;
            cb_contentTest_160k.Text = "160K";
            cb_contentTest_160k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_128k
            // 
            cb_contentTest_128k.AutoSize = true;
            cb_contentTest_128k.Checked = true;
            cb_contentTest_128k.CheckState = CheckState.Checked;
            cb_contentTest_128k.Location = new Point(610, 25);
            cb_contentTest_128k.Name = "cb_contentTest_128k";
            cb_contentTest_128k.Size = new Size(80, 28);
            cb_contentTest_128k.TabIndex = 0;
            cb_contentTest_128k.Text = "128K";
            cb_contentTest_128k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_96k
            // 
            cb_contentTest_96k.AutoSize = true;
            cb_contentTest_96k.Location = new Point(502, 25);
            cb_contentTest_96k.Name = "cb_contentTest_96k";
            cb_contentTest_96k.Size = new Size(69, 28);
            cb_contentTest_96k.TabIndex = 0;
            cb_contentTest_96k.Text = "96K";
            cb_contentTest_96k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_64k
            // 
            cb_contentTest_64k.AutoSize = true;
            cb_contentTest_64k.Location = new Point(394, 25);
            cb_contentTest_64k.Name = "cb_contentTest_64k";
            cb_contentTest_64k.Size = new Size(69, 28);
            cb_contentTest_64k.TabIndex = 0;
            cb_contentTest_64k.Text = "64K";
            cb_contentTest_64k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_32k
            // 
            cb_contentTest_32k.AutoSize = true;
            cb_contentTest_32k.Location = new Point(297, 25);
            cb_contentTest_32k.Name = "cb_contentTest_32k";
            cb_contentTest_32k.Size = new Size(69, 28);
            cb_contentTest_32k.TabIndex = 0;
            cb_contentTest_32k.Text = "32K";
            cb_contentTest_32k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_16k
            // 
            cb_contentTest_16k.AutoSize = true;
            cb_contentTest_16k.Location = new Point(201, 25);
            cb_contentTest_16k.Name = "cb_contentTest_16k";
            cb_contentTest_16k.Size = new Size(69, 28);
            cb_contentTest_16k.TabIndex = 0;
            cb_contentTest_16k.Text = "16K";
            cb_contentTest_16k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_8k
            // 
            cb_contentTest_8k.AutoSize = true;
            cb_contentTest_8k.Location = new Point(102, 25);
            cb_contentTest_8k.Name = "cb_contentTest_8k";
            cb_contentTest_8k.Size = new Size(58, 28);
            cb_contentTest_8k.TabIndex = 0;
            cb_contentTest_8k.Text = "8K";
            cb_contentTest_8k.UseVisualStyleBackColor = true;
            // 
            // cb_contentTest_4k
            // 
            cb_contentTest_4k.AutoSize = true;
            cb_contentTest_4k.Checked = true;
            cb_contentTest_4k.CheckState = CheckState.Checked;
            cb_contentTest_4k.Location = new Point(7, 25);
            cb_contentTest_4k.Name = "cb_contentTest_4k";
            cb_contentTest_4k.Size = new Size(58, 28);
            cb_contentTest_4k.TabIndex = 0;
            cb_contentTest_4k.Text = "4K";
            cb_contentTest_4k.UseVisualStyleBackColor = true;
            // 
            // tab_test_multithreading
            // 
            tab_test_multithreading.Controls.Add(tb_multithreadTest_request);
            tab_test_multithreading.Controls.Add(btn_multithreadTest_cpu);
            tab_test_multithreading.Controls.Add(groupBox3);
            tab_test_multithreading.Location = new Point(4, 33);
            tab_test_multithreading.Name = "tab_test_multithreading";
            tab_test_multithreading.Size = new Size(1186, 199);
            tab_test_multithreading.TabIndex = 2;
            tab_test_multithreading.Text = "3、多线程测试";
            tab_test_multithreading.UseVisualStyleBackColor = true;
            // 
            // tb_multithreadTest_request
            // 
            tb_multithreadTest_request.Enabled = true;
            tb_multithreadTest_request.Location = new Point(644, 113);
            tb_multithreadTest_request.Name = "tb_multithreadTest_request";
            tb_multithreadTest_request.Size = new Size(226, 49);
            tb_multithreadTest_request.TabIndex = 6;
            tb_multithreadTest_request.Text = "大模型同时请求测速";
            tb_multithreadTest_request.UseVisualStyleBackColor = true;
            tb_multithreadTest_request.Click += tb_multithreadTest_request_Click;
            // 
            // tab_test_advanced_a
            // 
            tab_test_advanced_a = new TabPage();
            gp_advanced_bench = new GroupBox();
            btn_runBench = new Button();
            label_bench_desc = new Label();
            gp_advanced_template = new GroupBox();
            btn_runTemplateAnalysis = new Button();
            label_template_desc = new Label();
            tab_test_advanced_b = new TabPage();
            gp_advanced_concurrent = new GroupBox();
            btn_runConcurrentTest = new Button();
            label_concurrent_desc = new Label();
            tb_concurrent_url = new TextBox();
            tb_concurrent_count = new TextBox();
            tb_concurrent_prompt = new TextBox();
            label_concurrent_url = new Label();
            label_concurrent_count = new Label();
            label_concurrent_prompt = new Label();
            gp_advanced_results = new GroupBox();
            btn_runResults = new Button();
            label_results_desc = new Label();

            tab_test_advanced_a.SuspendLayout();
            tab_test_advanced_b.SuspendLayout();
            gp_advanced_bench.SuspendLayout();
            gp_advanced_template.SuspendLayout();
            gp_advanced_concurrent.SuspendLayout();
            gp_advanced_results.SuspendLayout();

            // 
            // gp_advanced_bench
            // 
            gp_advanced_bench.Controls.Add(btn_runBench);
            gp_advanced_bench.Controls.Add(label_bench_desc);
            gp_advanced_bench.Location = new Point(8, 8);
            gp_advanced_bench.Name = "gp_advanced_bench";
            gp_advanced_bench.Size = new Size(1100, 100);
            gp_advanced_bench.TabIndex = 0;
            gp_advanced_bench.TabStop = false;
            gp_advanced_bench.Text = "1. llama-bench 性能基准";
            //
            // btn_runBench
            //
            btn_runBench.Location = new Point(15, 62);
            btn_runBench.Name = "btn_runBench";
            btn_runBench.Size = new Size(150, 30);
            btn_runBench.TabIndex = 1;
            btn_runBench.Text = "运行基准测试";
            btn_runBench.UseVisualStyleBackColor = true;
            btn_runBench.Click += btn_runBench_Click;
            //
            // label_bench_desc
            //
            label_bench_desc.AutoSize = false;
            label_bench_desc.Location = new Point(15, 22);
            label_bench_desc.Name = "label_bench_desc";
            label_bench_desc.Size = new Size(1050, 34);
            label_bench_desc.Text = "调用 llama-bench.exe 测试当前模型的 prompt 处理与 token 生成速度 — 输出矩阵：n_gpu_layers × batch_size × ubatch_size × n_threads × n_prompt × n_gen";
            // 
            // gp_advanced_template
            // 
            gp_advanced_template.Controls.Add(btn_runTemplateAnalysis);
            gp_advanced_template.Controls.Add(label_template_desc);
            gp_advanced_template.Location = new Point(8, 116);
            gp_advanced_template.Name = "gp_advanced_template";
            gp_advanced_template.Size = new Size(1100, 100);
            gp_advanced_template.TabIndex = 1;
            gp_advanced_template.TabStop = false;
            gp_advanced_template.Text = "2. llama-template-analysis 模板分析";
            //
            // btn_runTemplateAnalysis
            //
            btn_runTemplateAnalysis.Location = new Point(15, 62);
            btn_runTemplateAnalysis.Name = "btn_runTemplateAnalysis";
            btn_runTemplateAnalysis.Size = new Size(150, 30);
            btn_runTemplateAnalysis.TabIndex = 1;
            btn_runTemplateAnalysis.Text = "分析聊天模板";
            btn_runTemplateAnalysis.UseVisualStyleBackColor = true;
            btn_runTemplateAnalysis.Click += btn_runTemplateAnalysis_Click;
            //
            // label_template_desc
            //
            label_template_desc.AutoSize = false;
            label_template_desc.Location = new Point(15, 22);
            label_template_desc.Name = "label_template_desc";
            label_template_desc.Size = new Size(1050, 34);
            label_template_desc.Text = "调用 llama-template-analysis.exe 分析模型的 Jinja 聊天模板 — 输出提示词 token 化结果与格式化预览";
            // 
            // gp_advanced_concurrent
            // 
            gp_advanced_concurrent.Controls.Add(btn_runConcurrentTest);
            gp_advanced_concurrent.Controls.Add(label_concurrent_desc);
            gp_advanced_concurrent.Controls.Add(tb_concurrent_url);
            gp_advanced_concurrent.Controls.Add(tb_concurrent_count);
            gp_advanced_concurrent.Controls.Add(tb_concurrent_prompt);
            gp_advanced_concurrent.Controls.Add(label_concurrent_url);
            gp_advanced_concurrent.Controls.Add(label_concurrent_count);
            gp_advanced_concurrent.Controls.Add(label_concurrent_prompt);
            gp_advanced_concurrent.Location = new Point(8, 8);
            gp_advanced_concurrent.Name = "gp_advanced_concurrent";
            gp_advanced_concurrent.Size = new Size(1100, 165);
            gp_advanced_concurrent.TabIndex = 2;
            gp_advanced_concurrent.TabStop = false;
            gp_advanced_concurrent.Text = "3. llama-server 并发请求测试";
            // 
            // btn_runConcurrentTest
            // 
            btn_runConcurrentTest.Location = new Point(15, 126);
            btn_runConcurrentTest.Name = "btn_runConcurrentTest";
            btn_runConcurrentTest.Size = new Size(180, 30);
            btn_runConcurrentTest.TabIndex = 4;
            btn_runConcurrentTest.Text = "开始并发压测";
            btn_runConcurrentTest.UseVisualStyleBackColor = true;
            btn_runConcurrentTest.Click += btn_runConcurrentTest_Click;
            // 
            // tb_concurrent_url
            // 
            tb_concurrent_url.Location = new Point(110, 22);
            tb_concurrent_url.Name = "tb_concurrent_url";
            tb_concurrent_url.Size = new Size(400, 30);
            tb_concurrent_url.TabIndex = 1;
            tb_concurrent_url.Text = "http://127.0.0.1:8090/v1/chat/completions";
            // 
            // label_concurrent_url
            // 
            label_concurrent_url.AutoSize = true;
            label_concurrent_url.Location = new Point(15, 28);
            label_concurrent_url.Name = "label_concurrent_url";
            label_concurrent_url.Size = new Size(90, 24);
            label_concurrent_url.Text = "API 地址:";
            // 
            // tb_concurrent_count
            // 
            tb_concurrent_count.Location = new Point(640, 22);
            tb_concurrent_count.Name = "tb_concurrent_count";
            tb_concurrent_count.Size = new Size(90, 30);
            tb_concurrent_count.TabIndex = 2;
            tb_concurrent_count.Text = "8";
            //
            // label_concurrent_count
            //
            label_concurrent_count.AutoSize = true;
            label_concurrent_count.Location = new Point(535, 28);
            label_concurrent_count.Name = "label_concurrent_count";
            label_concurrent_count.Size = new Size(105, 24);
            label_concurrent_count.Text = "并发线程数:";
            //
            // tb_concurrent_prompt
            //
            tb_concurrent_prompt.Location = new Point(110, 60);
            tb_concurrent_prompt.Name = "tb_concurrent_prompt";
            tb_concurrent_prompt.Size = new Size(620, 30);
            tb_concurrent_prompt.TabIndex = 3;
            tb_concurrent_prompt.Text = "请用一句话介绍 llama.cpp 的 MTP 多 Token 预测。";
            // 
            // label_concurrent_prompt
            // 
            label_concurrent_prompt.AutoSize = true;
            label_concurrent_prompt.Location = new Point(15, 66);
            label_concurrent_prompt.Name = "label_concurrent_prompt";
            label_concurrent_prompt.Size = new Size(90, 24);
            label_concurrent_prompt.Text = "测试问题:";
            // 
            // label_concurrent_desc
            // 
            label_concurrent_desc.AutoSize = true;
            label_concurrent_desc.Location = new Point(15, 100);
            label_concurrent_desc.Name = "label_concurrent_desc";
            label_concurrent_desc.Size = new Size(900, 24);
            label_concurrent_desc.Text = "⚠️ 需要先点击「开启 llama.cpp 的 web 访问」并等待 server 启动完成，再「开始并发压测」";
            // 
            // gp_advanced_results
            // 
            gp_advanced_results.Controls.Add(btn_runResults);
            gp_advanced_results.Controls.Add(label_results_desc);
            gp_advanced_results.Location = new Point(8, 181);
            gp_advanced_results.Name = "gp_advanced_results";
            gp_advanced_results.Size = new Size(1100, 90);
            gp_advanced_results.TabIndex = 3;
            gp_advanced_results.TabStop = false;
            gp_advanced_results.Text = "4. llama-results 结果汇总";
            // 
            // btn_runResults
            // 
            btn_runResults.Location = new Point(15, 45);
            btn_runResults.Name = "btn_runResults";
            btn_runResults.Size = new Size(150, 30);
            btn_runResults.TabIndex = 1;
            btn_runResults.Text = "查看测试汇总";
            btn_runResults.UseVisualStyleBackColor = true;
            btn_runResults.Click += btn_runResults_Click;
            // 
            // label_results_desc
            // 
            label_results_desc.AutoSize = true;
            label_results_desc.Location = new Point(180, 50);
            label_results_desc.Name = "label_results_desc";
            label_results_desc.Size = new Size(370, 24);
            label_results_desc.Text = "查看最近一次批量测速/多线程/并发测试的汇总报告";

            // 
            // tab_test_advanced_a
            // 
            tab_test_advanced_a.Controls.Add(gp_advanced_bench);
            tab_test_advanced_a.Controls.Add(gp_advanced_template);
            tab_test_advanced_a.Location = new Point(4, 33);
            tab_test_advanced_a.Name = "tab_test_advanced_a";
            tab_test_advanced_a.Size = new Size(1186, 230);
            tab_test_advanced_a.TabIndex = 3;
            tab_test_advanced_a.Text = "4、高级工具 A";
            tab_test_advanced_a.UseVisualStyleBackColor = true;
            // 
            // tab_test_advanced_b
            // 
            tab_test_advanced_b.Controls.Add(gp_advanced_concurrent);
            tab_test_advanced_b.Controls.Add(gp_advanced_results);
            tab_test_advanced_b.Location = new Point(4, 33);
            tab_test_advanced_b.Name = "tab_test_advanced_b";
            tab_test_advanced_b.Size = new Size(1186, 280);
            tab_test_advanced_b.TabIndex = 4;
            tab_test_advanced_b.Text = "4、高级工具 B";
            tab_test_advanced_b.UseVisualStyleBackColor = true;

            tab_test_advanced_a.ResumeLayout(false);
            tab_test_advanced_b.ResumeLayout(false);
            gp_advanced_bench.ResumeLayout(false);
            gp_advanced_bench.PerformLayout();
            gp_advanced_template.ResumeLayout(false);
            gp_advanced_template.PerformLayout();
            gp_advanced_concurrent.ResumeLayout(false);
            gp_advanced_concurrent.PerformLayout();
            gp_advanced_results.ResumeLayout(false);
            gp_advanced_results.PerformLayout();

            // 把新 tab 加到容器
            tabControl_test.Controls.Add(tab_test_advanced_a);
            tabControl_test.Controls.Add(tab_test_advanced_b);
            // 
            // btn_multithreadTest_cpu
            // 
            btn_multithreadTest_cpu.Location = new Point(640, 38);
            btn_multithreadTest_cpu.Name = "btn_multithreadTest_cpu";
            btn_multithreadTest_cpu.Size = new Size(230, 55);
            btn_multithreadTest_cpu.TabIndex = 5;
            btn_multithreadTest_cpu.Text = "CPU多线程测速";
            btn_multithreadTest_cpu.UseVisualStyleBackColor = true;
            btn_multithreadTest_cpu.Click += btn_multithreadTest_cpu_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(tb_multithreadTest_step);
            groupBox3.Controls.Add(tb_multithreadTest_end);
            groupBox3.Controls.Add(tb_multithreadTest_start);
            groupBox3.Controls.Add(label12);
            groupBox3.Controls.Add(label13);
            groupBox3.Controls.Add(label11);
            groupBox3.Controls.Add(label10);
            groupBox3.Controls.Add(cb_multithreadTest_contentLength);
            groupBox3.Location = new Point(16, 14);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(593, 161);
            groupBox3.TabIndex = 3;
            groupBox3.TabStop = false;
            groupBox3.Text = "多线程配置";
            // 
            // tb_multithreadTest_step
            // 
            tb_multithreadTest_step.Location = new Point(168, 112);
            tb_multithreadTest_step.Name = "tb_multithreadTest_step";
            tb_multithreadTest_step.Size = new Size(186, 30);
            tb_multithreadTest_step.TabIndex = 3;
            tb_multithreadTest_step.Text = "1";
            // 
            // tb_multithreadTest_end
            // 
            tb_multithreadTest_end.Location = new Point(461, 71);
            tb_multithreadTest_end.Name = "tb_multithreadTest_end";
            tb_multithreadTest_end.Size = new Size(81, 30);
            tb_multithreadTest_end.TabIndex = 3;
            tb_multithreadTest_end.Text = "16";
            // 
            // tb_multithreadTest_start
            // 
            tb_multithreadTest_start.Location = new Point(168, 73);
            tb_multithreadTest_start.Name = "tb_multithreadTest_start";
            tb_multithreadTest_start.Size = new Size(186, 30);
            tb_multithreadTest_start.TabIndex = 3;
            tb_multithreadTest_start.Text = "1";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(355, 77);
            label12.Name = "label12";
            label12.Size = new Size(100, 24);
            label12.TabIndex = 2;
            label12.Text = "结束线程：";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(98, 115);
            label13.Name = "label13";
            label13.Size = new Size(64, 24);
            label13.TabIndex = 2;
            label13.Text = "步长：";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(62, 76);
            label11.Name = "label11";
            label11.Size = new Size(100, 24);
            label11.TabIndex = 2;
            label11.Text = "起始线程：";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(44, 36);
            label10.Name = "label10";
            label10.Size = new Size(118, 24);
            label10.TabIndex = 0;
            label10.Text = "最大上下文：";
            // 
            // cb_multithreadTest_contentLength
            // 
            cb_multithreadTest_contentLength.FormattingEnabled = true;
            cb_multithreadTest_contentLength.Items.AddRange(new object[] { "4096 (4K)", "8192 (8K)", "16384 (16K)", "32768 (32K)", "65536 (64K)", "98304 (96K)", "131072 (128K)", "163840 (160K)", "196608 (192K)", "229376 (224K)", "262144 (256K)", "294912 (288K)", "327680 (320K)", "360448 (352K)", "393216 (384K)", "425984 (416K)", "458752 (448K)", "491520 (480K)", "524288 (512K)", "557056 (544K)", "589824 (576K)", "622592 (608K)", "655360 (640K)", "688128 (672K)", "720896 (704K)", "753664 (736K)", "786432 (768K)", "819200 (800K)", "851968 (832K)", "884736 (864K)", "917504 (896K)", "950272 (928K)", "983040 (960K)", "1048576 (1M)", "2097152 (2M)", "5242880 (5M)" });
            cb_multithreadTest_contentLength.Location = new Point(168, 33);
            cb_multithreadTest_contentLength.Name = "cb_multithreadTest_contentLength";
            cb_multithreadTest_contentLength.Size = new Size(186, 32);
            cb_multithreadTest_contentLength.TabIndex = 1;
            cb_multithreadTest_contentLength.Text = "8192 (8K)";
            // 
            // toolTip
            // 
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 300;
            toolTip.ReshowDelay = 100;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { 显示主窗口ToolStripMenuItem, 退出ToolStripMenuItem });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new Size(181, 70);
            // 
            // 显示主窗口ToolStripMenuItem
            // 
            显示主窗口ToolStripMenuItem.Name = "显示主窗口ToolStripMenuItem";
            显示主窗口ToolStripMenuItem.Size = new Size(180, 30);
            显示主窗口ToolStripMenuItem.Text = "显示主窗口";
            显示主窗口ToolStripMenuItem.Click += new EventHandler(显示主窗口ToolStripMenuItem_Click);
            // 
            // 退出ToolStripMenuItem
            // 
            退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            退出ToolStripMenuItem.Size = new Size(180, 30);
            退出ToolStripMenuItem.Text = "退出";
            退出ToolStripMenuItem.Click += new EventHandler(退出ToolStripMenuItem_Click);
            // 
            // notifyIcon
            // 
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.Icon = SystemIcons.Application; // 使用系统应用程序图标
            notifyIcon.Text = "LlamaForms";
            notifyIcon.Visible = false;
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            // 
            // mainFrm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1878, 1222);
            Controls.Add(splitContainer_main);
            Name = "mainFrm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "llama.cpp本地启动器（多参数）、启动参数管理工具、最优启动参数测试、多尺寸上下文批量测速工具、CPU多线程批量测速工具   贤小二 @2026.05.05";
            FormClosing += new FormClosingEventHandler(mainFrm_FormClosing);
            Resize += new EventHandler(mainFrm_Resize);
            panel_left.ResumeLayout(false);
            panel_left_center.ResumeLayout(false);
            panel_left_bottom.ResumeLayout(false);
            gp_left_config.ResumeLayout(false);
            gp_left_config.PerformLayout();
            panel_left_top.ResumeLayout(false);
            gp_models.ResumeLayout(false);
            gp_models.PerformLayout();
            panel_right.ResumeLayout(false);
            panel_right__logs.ResumeLayout(false);
            tabControl_logs.ResumeLayout(false);
            tabPage_log_runLogs.ResumeLayout(false);
            tabPage_log_runLogs.PerformLayout();
            tabPage_log_testLogs.ResumeLayout(false);
            tabPage_log_testLogs.PerformLayout();
            tabPage_log_serverLogs.ResumeLayout(false);
            tabPage_log_serverLogs.PerformLayout();
            panel_right_status.ResumeLayout(false);
            panel_right_status.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer)).EndInit();
            splitContainer.ResumeLayout(false);
            tabControl_test.ResumeLayout(false);
            tab_test_startConfig.ResumeLayout(false);
            tab_test_startConfig.PerformLayout();
            tab_test_contentBatch.ResumeLayout(false);
            gp_content.ResumeLayout(false);
            gp_content.PerformLayout();
            tab_test_multithreading.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            splitContainer_main.Panel1.ResumeLayout(false);
            splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer_main)).EndInit();
            splitContainer_main.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel_left;
        private Panel panel_right;
        private Panel panel_left_top;
        private GroupBox gp_models;
        private Label label_llamaPath;
        private Button btn_llamaPath;
        private TextBox tb_llamaPath;
        private Label label_ggufModel;
        private ComboBox cb_ggufModel;
        private ToolTip toolTip;
        private Button btn_mmprojFile;
        private TextBox tb_mmprojFile;
        private Label label_mmprojFile;
        private Label label_mtpFile;
        private TextBox tb_mtpFile;
        private Button btn_mtpFile;
        private Button btn_ggufModel_Refresh;
        private Panel panel_left_center;
        private Panel panel_left_bottom;
        private SplitContainer splitContainer_left;
        private SplitContainer splitContainer_main;
        private GroupBox gp_left_config;
        private TextBox tb_box_webConfig;
        private Button btn_getOptions;
        private Button btn_CleanLogs;
        private Button btn_SaveBatchCmd;
        private Button btn_chatTest;
        private Button button_import_options;
        private Button button_save_options;
        private Panel panel_right_top;
        private SplitContainer splitContainer;
        private Panel panel_right__logs;
        private Panel panel_right_status;
        private TabControl tabControl_test;
        private TabPage tab_test_startConfig;
        private TabPage tab_test_contentBatch;
        private TabControl tabControl_logs;
        private TabPage tabPage_log_runLogs;
        private TabPage tabPage_log_testLogs;
        private TabPage tab_test_multithreading;
        private TextBox tb_TotalInfoLogs;
        private TextBox tb_runningLogs;
        private TabPage tabPage_log_serverLogs;
        private Button btn_modelID;
        private Button btn_apiKey;
        private Button btn_apiUrl;
        private TextBox tb_modelID;
        private TextBox tb_apiKey;
        private TextBox tb_apiUrl;
        private Label label8;
        private Label label6;
        private Label label5;
        private Button btn_openclaw_web;
        private CheckBox cb_startConfig_local;
        private GroupBox gp_content;
        private CheckBox cb_contentTest_5M;
        private CheckBox cb_contentTest_2M;
        private CheckBox cb_contentTest_1M;
        private CheckBox cb_contentTest_960k;
        private CheckBox cb_contentTest_928k;
        private CheckBox cb_contentTest_896k;
        private CheckBox cb_contentTest_864k;
        private CheckBox cb_contentTest_832k;
        private CheckBox cb_contentTest_800k;
        private CheckBox cb_contentTest_768k;
        private CheckBox cb_contentTest_736k;
        private CheckBox cb_contentTest_704k;
        private CheckBox cb_contentTest_672k;
        private CheckBox cb_contentTest_640k;
        private CheckBox cb_contentTest_608k;
        private CheckBox cb_contentTest_576k;
        private CheckBox cb_contentTest_544k;
        private CheckBox cb_contentTest_512k;
        private CheckBox cb_contentTest_480k;
        private CheckBox cb_contentTest_448k;
        private CheckBox cb_contentTest_416k;
        private CheckBox cb_contentTest_384k;
        private CheckBox cb_contentTest_352k;
        private CheckBox cb_contentTest_320k;
        private CheckBox cb_contentTest_288k;
        private CheckBox cb_contentTest_256k;
        private CheckBox cb_contentTest_224k;
        private CheckBox cb_contentTest_192k;
        private CheckBox cb_contentTest_160k;
        private CheckBox cb_contentTest_128k;
        private CheckBox cb_contentTest_96k;
        private CheckBox cb_contentTest_64k;
        private CheckBox cb_contentTest_32k;
        private CheckBox cb_contentTest_16k;
        private CheckBox cb_contentTest_8k;
        private CheckBox cb_contentTest_4k;
        private Button btn_contentTest_Run;
        private Button btn_contentTest_selectOther;
        private Button btn_contentTest_selectAll;
        private GroupBox groupBox3;
        private TextBox tb_multithreadTest_step;
        private TextBox tb_multithreadTest_end;
        private TextBox tb_multithreadTest_start;
        private Label label12;
        private Label label13;
        private Label label11;
        private Label label10;
        private ComboBox cb_multithreadTest_contentLength;
        private Button tb_multithreadTest_request;
        private Button btn_multithreadTest_cpu;
        // ── v2.4.0+ 高级工具 Tab ───────────────────────────────────────
        private TabPage tab_test_advanced_a;
        private TabPage tab_test_advanced_b;
        private GroupBox gp_advanced_bench;
        private Button btn_runBench;
        private Label label_bench_desc;
        private GroupBox gp_advanced_template;
        private Button btn_runTemplateAnalysis;
        private Label label_template_desc;
        private GroupBox gp_advanced_concurrent;
        private Button btn_runConcurrentTest;
        private Label label_concurrent_desc;
        private TextBox tb_concurrent_url;
        private TextBox tb_concurrent_count;
        private TextBox tb_concurrent_prompt;
        private Label label_concurrent_url;
        private Label label_concurrent_count;
        private Label label_concurrent_prompt;
        private GroupBox gp_advanced_results;
        private Button btn_runResults;
        private Label label_results_desc;
        private TextBox tb_serverLogs;
        private Button btn_analyze_options;
        private Button btn_ggufModelsPath;
        private TextBox tb_ggufModelsPath;
        private GroupBox gp_model_options;
        private Label label1;
        private TextBox tb_apiUrl_port;
        private TextBox tb_apiUrl_address;
        private Label label2;
        private CheckBox cb_startConfig_cors;
        private Label label3;
        private TextBox tb_tokens;
        private Panel panel_status;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem 显示主窗口ToolStripMenuItem;
        private ToolStripMenuItem 退出ToolStripMenuItem;
    }
}
