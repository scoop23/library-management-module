using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class ClearanceForm : Form
    {
        private readonly BorrowService _borrowService = new();
        private readonly InternalStudentService _studentService = new();

        private TextBox txtStudentNumber;
        private Button btnCheck, btnPrint, btnClose;

        // Student Card Controls
        private Panel pnlStudentCard;
        private Label lblAvatar, lblStudentName, lblStudentDetailsLeft, lblStudentDetailsRight;

        // Results & Status
        private Panel pnlResult, pnlBanner;
        private Label lblBannerText, lblStatus, lblStatusDetail;
        private DataGridView dgvActiveBorrows, dgvAllBorrows;

        public ClearanceForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form Setup
            Text = "Library Clearance System";
            Width = 860;
            Height = 900;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(248, 249, 250);
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

            // ==========================================
            // 1. HEADER PANEL
            // ==========================================
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(21, 67, 140) 
            };

            var lblTitle = new Label
            {
                Text = "Library Clearance Status",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 16),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            // ==========================================
            // 2. SEARCH BAR PANEL
            // ==========================================
            var pnlSearchCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 15)
            };

            var lblPrompt = new Label
            {
                Text = "Student ID / SrCode:",
                Location = new Point(20, 24),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            txtStudentNumber = new TextBox
            {
                Location = new Point(160, 20),
                Width = 280,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "e.g. 26-00001"
            };

            btnCheck = new Button
            {
                Text = "Check Clearance",
                Location = new Point(450, 18),
                Width = 150,
                Height = 34,
                BackColor = Color.FromArgb(21, 67, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            btnCheck.FlatAppearance.BorderSize = 0;
            btnCheck.Click += BtnCheck_Click;

            pnlSearchCard.Controls.AddRange(new Control[] { lblPrompt, txtStudentNumber, btnCheck });

            // Horizontal Line Divider
            var pnlDivider = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(226, 232, 240)
            };

            // ==========================================
            // 3. MAIN RESULTS CONTAINER
            // ==========================================
            pnlResult = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = Color.FromArgb(248, 249, 250),
                AutoScroll = true,
                Padding = new Padding(20)
            };

            // --- Status Banner ---
            pnlBanner = new Panel
            {
                Location = new Point(20, 15),
                Width = 800,
                Height = 55,
                BackColor = Color.FromArgb(34, 197, 94), // Emerald Green
                Visible = false
            };

            lblBannerText = new Label
            {
                Text = "CLEARED",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlBanner.Controls.Add(lblBannerText);
            pnlResult.Controls.Add(pnlBanner);

            // --- Student Information Card ---
            pnlStudentCard = new Panel
            {
                Location = new Point(20, 85),
                Width = 800,
                Height = 105,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var pnlAccent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = Color.FromArgb(37, 99, 235)
            };

            lblAvatar = new Label
            {
                Text = "👤",
                Font = new Font("Segoe UI Emoji", 28F),
                Location = new Point(15, 20),
                Size = new Size(50, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(148, 163, 184)
            };

            lblStudentName = new Label
            {
                Location = new Point(75, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            lblStudentDetailsLeft = new Label
            {
                Location = new Point(75, 42),
                Width = 350,
                Height = 55,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            lblStudentDetailsRight = new Label
            {
                Location = new Point(440, 42),
                Width = 340,
                Height = 55,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            pnlStudentCard.Controls.AddRange(new Control[] { pnlAccent, lblAvatar, lblStudentName, lblStudentDetailsLeft, lblStudentDetailsRight });
            pnlResult.Controls.Add(pnlStudentCard);

            // --- Status Summary ---
            lblStatus = new Label
            {
                Location = new Point(20, 205),
                Width = 800,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true
            };
            pnlResult.Controls.Add(lblStatus);

            lblStatusDetail = new Label
            {
                Location = new Point(20, 232),
                Width = 800,
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 9.5F)
            };
            pnlResult.Controls.Add(lblStatusDetail);

            // --- Active Borrows Grid Section ---
            var lblActiveHeader = new Label
            {
                Text = "Active Borrows (Clearance Blockers)",
                Location = new Point(20, 270),
                Width = 400,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(225, 29, 72) // Rose Red
            };
            pnlResult.Controls.Add(lblActiveHeader);

            dgvActiveBorrows = CreateStyledDataGrid();
            dgvActiveBorrows.Location = new Point(20, 298);
            dgvActiveBorrows.Width = 800;
            dgvActiveBorrows.Height = 125;
            pnlResult.Controls.Add(dgvActiveBorrows);

            // --- All Borrow History Grid Section ---
            var lblAllHeader = new Label
            {
                Text = "Borrowing History Log",
                Location = new Point(20, 440),
                Width = 400,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235)
            };
            pnlResult.Controls.Add(lblAllHeader);

            dgvAllBorrows = CreateStyledDataGrid();
            dgvAllBorrows.Location = new Point(20, 468);
            dgvAllBorrows.Width = 800;
            dgvAllBorrows.Height = 150;
            pnlResult.Controls.Add(dgvAllBorrows);

            // --- Action Buttons Bar ---
            btnPrint = new Button
            {
                Text = "🖨️  Print Clearance Certificate",
                Location = new Point(20, 638),
                Width = 230,
                Height = 40,
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Enabled = false
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += BtnPrint_Click;
            pnlResult.Controls.Add(btnPrint);

            // --- Bottom Close Button ---
            btnClose = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);

            // Add all layout components to form
            Controls.AddRange(new Control[] { pnlResult, pnlDivider, pnlSearchCard, pnlHeader, btnClose });
            AcceptButton = btnCheck;
        }

        // Helper Method for Modern DataGrid Styling
        private DataGridView CreateStyledDataGrid()
        {
            var dgv = new DataGridView
            {
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                RowTemplate = { Height = 32 }
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
            dgv.ColumnHeadersHeight = 35;

            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);

            return dgv;
        }

        private async void BtnCheck_Click(object sender, EventArgs e)
        {
            var studentNumber = txtStudentNumber.Text.Trim();
            if (string.IsNullOrWhiteSpace(studentNumber))
            {
                MessageBox.Show("Please enter a valid student number.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                btnCheck.Enabled = false;

                var student = await _studentService.GetStudentByNumberAsync(studentNumber);
                if (student == null)
                {
                    MessageBox.Show($"No student record found for ID '{studentNumber}'.", "Student Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    pnlResult.Visible = false;
                    return;
                }

                var activeBorrows = await _borrowService.GetActiveBorrowsByStudentAsync(student.StudentId);
                var allBorrows = await _borrowService.GetBorrowsByStudentAsync(student.StudentId);
                var existingClearance = await _studentService.GetClearanceAsync(student.StudentId);
                var previousStatus = existingClearance?.Status ?? "None";

                pnlResult.Visible = true;

                // Populate Student Card UI
                lblStudentName.Text = $"{student.LastName}, {student.FirstName} {student.MiddleName}".Trim();
                lblStudentDetailsLeft.Text = $"• Student ID  :  {student.StudentId}\n" +
                                             $"• Program     :  {student.ProgramId}\n" +
                                             $"• Year Level  :  {student.YearLevel}";

                lblStudentDetailsRight.Text = $"• Email       :  {student.Email}\n" +
                                              $"• Account Status :  {student.Status}\n" +
                                              $"• Last Clearance :  {previousStatus}";

                var clearanceRecord = new ClearanceRecord
                {
                    StudentId = student.StudentId,
                    LastChecked = DateTime.Now
                };

                // Clear Evaluation Logic
                if (activeBorrows.Count == 0 && student.Status == "Active")
                {
                    pnlBanner.Visible = true;
                    pnlBanner.BackColor = Color.FromArgb(34, 197, 94); // Emerald Green
                    lblBannerText.Text = "✓ CLEARED";
                    lblStatus.Text = "Clearance Status: APPROVED";
                    lblStatus.ForeColor = Color.FromArgb(22, 101, 52);
                    lblStatusDetail.Text = "Student has zero outstanding active borrows. Account is eligible for clearance printout.";
                    btnPrint.Enabled = true;
                    clearanceRecord.Status = "Cleared";
                }
                else if (activeBorrows.Count > 0)
                {
                    pnlBanner.Visible = true;
                    pnlBanner.BackColor = Color.FromArgb(225, 29, 72); // Crimson Red
                    lblBannerText.Text = "✕ NOT CLEARED";
                    lblStatus.Text = "Clearance Status: BLOCKED";
                    lblStatus.ForeColor = Color.FromArgb(159, 18, 57);
                    lblStatusDetail.Text = $"Student has {activeBorrows.Count} active borrow(s). All books must be returned prior to clearance approval.";
                    btnPrint.Enabled = false;
                    clearanceRecord.Status = "Not Cleared";
                }
                else
                {
                    pnlBanner.Visible = true;
                    pnlBanner.BackColor = Color.FromArgb(225, 29, 72);
                    lblBannerText.Text = "✕ NOT CLEARED";
                    lblStatus.Text = "Clearance Status: BLOCKED";
                    lblStatus.ForeColor = Color.FromArgb(159, 18, 57);
                    lblStatusDetail.Text = $"Student account status is '{student.Status}'. Only active accounts can receive clearance.";
                    btnPrint.Enabled = false;
                    clearanceRecord.Status = "Not Cleared";
                }

                await _studentService.SaveClearanceAsync(clearanceRecord);

                // Bind DataGrids
                dgvActiveBorrows.DataSource = activeBorrows
                    .Select(b => new
                    {
                        b.BookTitle,
                        BorrowDate = b.BorrowDate.ToString("yyyy-MM-dd"),
                        DueDate = b.DueDate.ToString("yyyy-MM-dd"),
                        b.Status
                    })
                    .ToList();

                dgvAllBorrows.DataSource = allBorrows
                    .Select(b => new
                    {
                        b.BookTitle,
                        BorrowDate = b.BorrowDate.ToString("yyyy-MM-dd"),
                        DueDate = b.DueDate.ToString("yyyy-MM-dd"),
                        ReturnDate = b.ReturnDate?.ToString("yyyy-MM-dd") ?? "—",
                        b.Status
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Clearance check failed: {ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnCheck.Enabled = true;
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            var printDoc = new PrintDocument();
            printDoc.PrintPage += (s, pe) =>
            {
                float y = 50;
                var titleFont = new Font("Segoe UI", 18, FontStyle.Bold);
                var headerFont = new Font("Segoe UI", 11, FontStyle.Bold);
                var bodyFont = new Font("Segoe UI", 10);

                pe.Graphics.DrawString("OFFICIAL LIBRARY CLEARANCE", titleFont, Brushes.Black, new PointF(180, y));
                y += 45;
                pe.Graphics.DrawString($"Issued Date: {DateTime.Now:MMMM dd, yyyy}", bodyFont, Brushes.Black, new PointF(40, y));
                y += 25;
                pe.Graphics.DrawString("─".PadRight(80, '─'), bodyFont, Brushes.Gray, new PointF(40, y));
                y += 25;

                string printStudentInfo = $"Student Name : {lblStudentName.Text}\n\n" +
                                         $"{lblStudentDetailsLeft.Text.Replace("• ", "")}\n" +
                                         $"{lblStudentDetailsRight.Text.Replace("• ", "")}";

                pe.Graphics.DrawString(printStudentInfo, bodyFont, Brushes.Black, new PointF(40, y));
                y += 120;
                pe.Graphics.DrawString("─".PadRight(80, '─'), bodyFont, Brushes.Gray, new PointF(40, y));
                y += 25;

                pe.Graphics.DrawString($"{lblStatus.Text}", headerFont, Brushes.Black, new PointF(40, y));
                y += 25;
                pe.Graphics.DrawString(lblStatusDetail.Text, bodyFont, Brushes.Black, new PointF(40, y));
                y += 50;
                pe.Graphics.DrawString("─".PadRight(80, '─'), bodyFont, Brushes.Gray, new PointF(40, y));
                y += 50;
                pe.Graphics.DrawString("Head Librarian Signature: ___________________", bodyFont, Brushes.Black, new PointF(40, y));
                y += 35;
                pe.Graphics.DrawString("Student Signature:          ___________________", bodyFont, Brushes.Black, new PointF(40, y));
            };

            using var preview = new PrintPreviewDialog { Document = printDoc, Width = 850, Height = 650 };
            preview.ShowDialog(this);
        }
    }
}