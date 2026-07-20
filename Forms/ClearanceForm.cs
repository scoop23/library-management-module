using System.Drawing;
using System.Drawing.Printing;
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
        private Label lblStudentInfo, lblStatus, lblStatusDetail;
        private DataGridView dgvActiveBorrows, dgvAllBorrows;
        private Panel pnlResult, pnlBanner;
        private Label lblBannerText;

        public ClearanceForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Library Clearance";
            Width = 820;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(21, 67, 140)
            };

            var lblTitle = new Label
            {
                Text = "Library Clearance",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 13),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var lblPrompt = new Label { Text = "Student Number:", Location = new Point(12, 16), Width = 110, Font = new Font("Segoe UI", 9) };
            txtStudentNumber = new TextBox { Location = new Point(127, 12), Width = 300, PlaceholderText = "e.g. 26-00001" };

            btnCheck = new Button
            {
                Text = "Check Clearance",
                Location = new Point(445, 10),
                Width = 150,
                Height = 32,
                BackColor = Color.FromArgb(21, 67, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCheck.Click += BtnCheck_Click;

            pnlSearch.Controls.AddRange(new Control[] { lblPrompt, txtStudentNumber, btnCheck });

            pnlResult = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10),
                AutoScroll = true
            };

            pnlBanner = new Panel
            {
                Location = new Point(10, 10),
                Width = 760,
                Height = 60,
                BackColor = Color.FromArgb(46, 139, 87),
                Visible = false
            };

            lblBannerText = new Label
            {
                Text = "CLEARED",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlBanner.Controls.Add(lblBannerText);
            pnlResult.Controls.Add(pnlBanner);

            lblStudentInfo = new Label
            {
                Location = new Point(10, 80),
                Width = 760,
                Height = 55,
                Font = new Font("Segoe UI", 9)
            };
            pnlResult.Controls.Add(lblStudentInfo);

            lblStatus = new Label
            {
                Location = new Point(10, 140),
                Width = 760,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true
            };
            pnlResult.Controls.Add(lblStatus);

            lblStatusDetail = new Label
            {
                Location = new Point(10, 175),
                Width = 760,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9)
            };
            pnlResult.Controls.Add(lblStatusDetail);

            var lblActiveHeader = new Label
            {
                Text = "Active Borrows (Blocking Clearance):",
                Location = new Point(10, 210),
                Width = 400,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(178, 34, 34)
            };
            pnlResult.Controls.Add(lblActiveHeader);

            dgvActiveBorrows = new DataGridView
            {
                Location = new Point(10, 235),
                Width = 760,
                Height = 120,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlResult.Controls.Add(dgvActiveBorrows);

            var lblAllHeader = new Label
            {
                Text = "All Borrow History:",
                Location = new Point(10, 370),
                Width = 400,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(21, 67, 140)
            };
            pnlResult.Controls.Add(lblAllHeader);

            dgvAllBorrows = new DataGridView
            {
                Location = new Point(10, 395),
                Width = 760,
                Height = 160,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlResult.Controls.Add(dgvAllBorrows);

            btnPrint = new Button
            {
                Text = "Print Clearance",
                Location = new Point(10, 570),
                Width = 150,
                Height = 36,
                BackColor = Color.FromArgb(46, 139, 87),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnPrint.Click += BtnPrint_Click;
            pnlResult.Controls.Add(btnPrint);

            btnClose = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            Controls.AddRange(new Control[] { pnlHeader, pnlSearch, pnlResult, btnClose });
            AcceptButton = btnCheck;
        }

        private async void BtnCheck_Click(object sender, EventArgs e)
        {
            var studentNumber = txtStudentNumber.Text.Trim();
            if (string.IsNullOrWhiteSpace(studentNumber))
            {
                MessageBox.Show("Please enter a student number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                btnCheck.Enabled = false;

                var student = await _studentService.GetStudentByNumberAsync(studentNumber);
                if (student == null)
                {
                    MessageBox.Show($"No student found with number '{studentNumber}'.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    pnlResult.Visible = false;
                    return;
                }

                var activeBorrows = await _borrowService.GetActiveBorrowsByStudentAsync(student.StudentId);
                var allBorrows = await _borrowService.GetBorrowsByStudentAsync(student.StudentId);
                var existingClearance = await _studentService.GetClearanceAsync(student.StudentId);
                var previousStatus = existingClearance?.Status ?? "Not Cleared";

                pnlResult.Visible = true;
                lblStudentInfo.Text = $"Student: {student.LastName}, {student.FirstName} {student.MiddleName}\n" +
                                       $"SrCode: {student.StudentId}  |  Program: {student.ProgramId}  |  Year: {student.YearLevel}\n" +
                                       $"Email: {student.Email}  |  Status: {student.Status}  |  Previous Clearance: {previousStatus}";

                var clearanceRecord = new ClearanceRecord
                {
                    StudentId = student.StudentId,
                    LastChecked = DateTime.Now
                };

                if (activeBorrows.Count == 0 && student.Status == "Active")
                {
                    pnlBanner.Visible = true;
                    pnlBanner.BackColor = Color.FromArgb(46, 139, 87);
                    lblBannerText.Text = "CLEARED";
                    lblStatus.Text = "Status: CLEARED";
                    lblStatus.ForeColor = Color.FromArgb(46, 139, 87);
                    lblStatusDetail.Text = "No active borrows. Student is eligible for library clearance.";
                    btnPrint.Enabled = true;
                    clearanceRecord.Status = "Cleared";
                }
                else if (activeBorrows.Count > 0)
                {
                    pnlBanner.Visible = true;
                    pnlBanner.BackColor = Color.FromArgb(178, 34, 34);
                    lblBannerText.Text = "NOT CLEARED";
                    lblStatus.Text = "Status: NOT CLEARED";
                    lblStatus.ForeColor = Color.Red;
                    lblStatusDetail.Text = $"Student has {activeBorrows.Count} active borrow(s). All books must be returned before clearance.";
                    btnPrint.Enabled = false;
                    clearanceRecord.Status = "Not Cleared";
                }
                else
                {
                    pnlBanner.Visible = true;
                    pnlBanner.BackColor = Color.FromArgb(178, 34, 34);
                    lblBannerText.Text = "NOT CLEARED";
                    lblStatus.Text = "Status: NOT CLEARED";
                    lblStatus.ForeColor = Color.Red;
                    lblStatusDetail.Text = $"Student status is '{student.Status}'. Only active students can be cleared.";
                    btnPrint.Enabled = false;
                    clearanceRecord.Status = "Not Cleared";
                }

                await _studentService.SaveClearanceAsync(clearanceRecord);

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
                MessageBox.Show($"Failed to check clearance: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                float y = 40;
                var titleFont = new Font("Segoe UI", 18, FontStyle.Bold);
                var headerFont = new Font("Segoe UI", 11, FontStyle.Bold);
                var bodyFont = new Font("Segoe UI", 10);

                pe.Graphics.DrawString("LIBRARY CLEARANCE", titleFont, Brushes.Black, new PointF(200, y));
                y += 40;
                pe.Graphics.DrawString($"Date: {DateTime.Now:MMMM dd, yyyy}", bodyFont, Brushes.Black, new PointF(40, y));
                y += 25;
                pe.Graphics.DrawString("─".PadRight(80, '─'), bodyFont, Brushes.Gray, new PointF(40, y));
                y += 20;

                pe.Graphics.DrawString(lblStudentInfo.Text, headerFont, Brushes.Black, new PointF(40, y));
                y += 70;
                pe.Graphics.DrawString("─".PadRight(80, '─'), bodyFont, Brushes.Gray, new PointF(40, y));
                y += 20;

                pe.Graphics.DrawString($"Status: {lblStatus.Text}", headerFont, Brushes.Black, new PointF(40, y));
                y += 25;
                pe.Graphics.DrawString(lblStatusDetail.Text, bodyFont, Brushes.Black, new PointF(40, y));
                y += 40;
                pe.Graphics.DrawString("─".PadRight(80, '─'), bodyFont, Brushes.Gray, new PointF(40, y));
                y += 30;
                pe.Graphics.DrawString("Librarian Signature: ___________________", bodyFont, Brushes.Black, new PointF(40, y));
                y += 25;
                pe.Graphics.DrawString("Student Signature:   ___________________", bodyFont, Brushes.Black, new PointF(40, y));
            };

            using var preview = new PrintPreviewDialog { Document = printDoc, Width = 800, Height = 600 };
            preview.ShowDialog(this);
        }
    }
}
