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
        private DataGridView dgvActiveBorrows;
        private Panel pnlResult;

        public ClearanceForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Library Clearance";
            Width = 700;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            var lblTitle = new Label
            {
                Text = "Library Clearance",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(21, 67, 140),
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblPrompt = new Label { Text = "Enter Student Number:", Location = new Point(20, 60), Width = 150 };
            txtStudentNumber = new TextBox { Location = new Point(180, 57), Width = 250, PlaceholderText = "e.g. STU-2026-001" };

            btnCheck = new Button
            {
                Text = "Check Clearance",
                Location = new Point(450, 55),
                Width = 130,
                Height = 30,
                BackColor = Color.FromArgb(21, 67, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCheck.Click += BtnCheck_Click;

            pnlResult = new Panel { Location = new Point(20, 100), Width = 640, Height = 350, Visible = false };

            lblStudentInfo = new Label { Location = new Point(0, 0), Width = 640, Font = new Font("Segoe UI", 10) };
            pnlResult.Controls.Add(lblStudentInfo);

            lblStatus = new Label { Location = new Point(0, 50), Width = 640, Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true };
            pnlResult.Controls.Add(lblStatus);

            lblStatusDetail = new Label { Location = new Point(0, 85), Width = 640, ForeColor = Color.Gray };
            pnlResult.Controls.Add(lblStatusDetail);

            var lblActiveHeader = new Label { Text = "Active Borrows:", Location = new Point(0, 120), Width = 200, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            pnlResult.Controls.Add(lblActiveHeader);

            dgvActiveBorrows = new DataGridView
            {
                Location = new Point(0, 145),
                Width = 640,
                Height = 160,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlResult.Controls.Add(dgvActiveBorrows);

            btnPrint = new Button
            {
                Text = "Print Clearance",
                Location = new Point(0, 315),
                Width = 140,
                Height = 30,
                BackColor = Color.FromArgb(46, 139, 87),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnPrint.Click += BtnPrint_Click;
            pnlResult.Controls.Add(btnPrint);

            btnClose = new Button { Text = "Close", Location = new Point(560, 440), Width = 100, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };

            Controls.AddRange(new Control[] { lblTitle, lblPrompt, txtStudentNumber, btnCheck, pnlResult, btnClose });
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

                pnlResult.Visible = true;
                lblStudentInfo.Text = $"Student: {student.LastName}, {student.FirstName}\n" +
                                      $"SrCode: {student.StudentId}  | Year: {student.YearLevel}\n" +
                                      //$"SrCode: {student.StudentId}  |  Dept: {student.Department}  |  Year: {student.YearLevel}\n" +
                                      $"Email: {student.Email}  |  Status: {student.Status}";

                if (activeBorrows.Count == 0 && student.Status == "Active")
                {
                    lblStatus.Text = "CLEARED";
                    lblStatus.ForeColor = Color.FromArgb(46, 139, 87);
                    lblStatusDetail.Text = "No active borrows. Student is eligible for library clearance.";
                    btnPrint.Enabled = true;
                }
                else if (activeBorrows.Count > 0)
                {
                    lblStatus.Text = "NOT CLEARED";
                    lblStatus.ForeColor = Color.FromArgb(178, 34, 34);
                    lblStatusDetail.Text = $"Student has {activeBorrows.Count} active borrow(s). All books must be returned before clearance.";
                    btnPrint.Enabled = false;
                }
                else
                {
                    lblStatus.Text = "NOT CLEARED";
                    lblStatus.ForeColor = Color.FromArgb(178, 34, 34);
                    lblStatusDetail.Text = $"Student status is '{student.Status}'. Only active students can be cleared.";
                    btnPrint.Enabled = false;
                }

                dgvActiveBorrows.DataSource = activeBorrows
                    .Select(b => new
                    {
                        b.BookTitle,
                        BorrowDate = b.BorrowDate.ToString("yyyy-MM-dd"),
                        DueDate = b.DueDate.ToString("yyyy-MM-dd"),
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
            var studentNumber = txtStudentNumber.Text.Trim();
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

                pe.Graphics.DrawString("Status: CLEARED", headerFont, Brushes.Black, new PointF(40, y));
                y += 25;
                pe.Graphics.DrawString("No outstanding library obligations.", bodyFont, Brushes.Black, new PointF(40, y));
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
