using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace KeyCapture
{
    // Graphics 확장 메서드 (둥근 모서리 사각형을 그리기 위해)
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, int x, int y, int width, int height, int radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
                path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
                path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseAllFigures();
                graphics.FillPath(brush, path);
            }
        }
    }


    public partial class KeyCapture : Form
    {
        private bool isSelecting = false;
        private Point startPoint;
        private Point endPoint;
        private Rectangle selectedArea;
        private OverlayForm overlayForm;
        private string saveDirectory;
        private int captureCount = 0;

        // Windows API for custom title bar
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, uint attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        public KeyCapture()
        {
            InitializeComponent();
            SetupSaveDirectory();
            RegisterGlobalHotkey();
            //SetCustomTitleBarColor();
            //SetProgramIcon();
            SetupPersistentIcon();

            string programFolder = Application.StartupPath;
            string iconPath1 = Path.Combine(programFolder, "KeyCapture.ico");

            if (File.Exists(iconPath1))
            {
                this.Icon = new Icon(iconPath1);
                return;
            }

            // this.Icon = Properties.Resources.AppIcon;

        }

        // 작업 표시줄 아이콘 문제 해결을 위한 추가 메서드
        //public void ForceTaskbarIconRefresh()
        //{
        //    try
        //    {
        //        // 아이콘을 null로 설정 후 다시 설정
        //        Icon currentIcon = this.Icon;
        //        this.Icon = null;
        //        Application.DoEvents();
        //        this.Icon = currentIcon;

        //        // Windows에 아이콘 변경 알림
        //        RefreshTaskbarIcon();

        //        // 약간의 지연 후 한 번 더 시도
        //        System.Threading.Timer timer = new System.Threading.Timer(_ =>
        //        {
        //            this.BeginInvoke(new Action(() =>
        //            {
        //                RefreshTaskbarIcon();
        //            }));
        //        }, null, 100, System.Threading.Timeout.Infinite);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"강제 새로고침 실패: {ex.Message}");
        //    }
        //}

        private void SetupPersistentIcon()
        {
            try
            {
                // 실행 파일과 같은 위치에 아이콘 파일 생성/복사
                // CreatePersistentIconFile();

                // 레지스트리에 아이콘 경로 등록 (선택사항)
                // RegisterIconInRegistry();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"영구 아이콘 설정 실패: {ex.Message}");
            }
        }

        private void CreatePersistentIconFile()
        {
            try
            {
                string exePath = Application.ExecutablePath;
                string iconPath = Path.ChangeExtension(exePath, ".ico");

                // 아이콘 파일이 이미 존재하고 최신이면 건너뛰기
                if (File.Exists(iconPath))
                {
                    DateTime exeTime = File.GetLastWriteTime(exePath);
                    DateTime iconTime = File.GetLastWriteTime(iconPath);
                    if (iconTime >= exeTime)
                    {
                        return; // 아이콘이 최신 상태
                    }
                }

                // 현재 프로그램 아이콘을 파일로 저장
                if (this.Icon != null)
                {
                    using (FileStream fs = new FileStream(iconPath, FileMode.Create))
                    {
                        this.Icon.Save(fs);
                    }
                    Debug.WriteLine($"영구 아이콘 파일 생성: {iconPath}");
                }
                else
                {
                    // 아이콘이 없으면 기본 아이콘 생성 후 저장
                    Icon defaultIcon = CreateMultiSizeIcon();
                    using (FileStream fs = new FileStream(iconPath, FileMode.Create))
                    {
                        defaultIcon.Save(fs);
                    }
                    Debug.WriteLine($"기본 아이콘 파일 생성: {iconPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"아이콘 파일 생성 실패: {ex.Message}");
            }
        }

        private void RegisterIconInRegistry()
        {
            try
            {
                // Windows 레지스트리에 프로그램 정보 등록
                string exePath = Application.ExecutablePath;
                string iconPath = Path.ChangeExtension(exePath, ".ico");
                string appName = Path.GetFileNameWithoutExtension(exePath);
                string executableName = Path.GetFileName(exePath);

                // 1. HKEY_CLASSES_ROOT에 프로그램 정보 등록 (전체 시스템)
                try
                {
                    using (var key = Registry.ClassesRoot.CreateSubKey($"Applications\\{executableName}"))
                    {
                        if (key != null)
                        {
                            key.SetValue("", "화면 캡처 프로그램");
                            key.SetValue("FriendlyAppName", "Screen Capture Tool");

                            // 기본 아이콘 등록
                            using (var defaultIconKey = key.CreateSubKey("DefaultIcon"))
                            {
                                if (defaultIconKey != null && File.Exists(iconPath))
                                {
                                    defaultIconKey.SetValue("", $"{iconPath},0");
                                }
                                else
                                {
                                    // 아이콘 파일이 없으면 실행파일의 아이콘 사용
                                    defaultIconKey.SetValue("", $"{exePath},0");
                                }
                            }

                            // 지원되는 파일 형식 등록 (선택사항)
                            using (var supportedTypesKey = key.CreateSubKey("SupportedTypes"))
                            {
                                if (supportedTypesKey != null)
                                {
                                    supportedTypesKey.SetValue(".png", "");
                                    supportedTypesKey.SetValue(".jpg", "");
                                    supportedTypesKey.SetValue(".jpeg", "");
                                    supportedTypesKey.SetValue(".bmp", "");
                                }
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("HKEY_CLASSES_ROOT 등록 완료");
                }
                catch (UnauthorizedAccessException)
                {
                    System.Diagnostics.Debug.WriteLine("HKEY_CLASSES_ROOT 접근 권한 없음 (관리자 권한 필요)");
                }

                // 2. 현재 사용자의 프로그램 등록 (관리자 권한 불필요)
                try
                {
                    using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\Applications\{executableName}"))
                    {
                        if (key != null)
                        {
                            key.SetValue("", "화면 캡처 프로그램");
                            key.SetValue("FriendlyAppName", "Screen Capture Tool");
                            key.SetValue("ApplicationDescription", "마우스로 화면 영역을 선택하여 캡처하는 도구");
                            key.SetValue("ApplicationCompany", "Custom Tools");

                            // 기본 아이콘 등록
                            using (var defaultIconKey = key.CreateSubKey("DefaultIcon"))
                            {
                                if (defaultIconKey != null && File.Exists(iconPath))
                                {
                                    defaultIconKey.SetValue("", $"{iconPath},0");
                                }
                                else
                                {
                                    defaultIconKey.SetValue("", $"{exePath},0");
                                }
                            }

                            // Shell 명령어 등록
                            using (var shellKey = key.CreateSubKey("shell"))
                            {
                                if (shellKey != null)
                                {
                                    using (var openKey = shellKey.CreateSubKey("open"))
                                    {
                                        if (openKey != null)
                                        {
                                            openKey.SetValue("", "열기(&O)");
                                            using (var commandKey = openKey.CreateSubKey("command"))
                                            {
                                                if (commandKey != null)
                                                {
                                                    commandKey.SetValue("", $"\"{exePath}\" \"%1\"");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("HKEY_CURRENT_USER 등록 완료");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"사용자 레지스트리 등록 실패: {ex.Message}");
                }

                // 3. 프로그램 목록에 등록 (프로그램 추가/제거에 표시)
                try
                {
                    string uninstallKey = $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{appName}";
                    using (var key = Registry.CurrentUser.CreateSubKey(uninstallKey))
                    {
                        if (key != null)
                        {
                            key.SetValue("DisplayName", "화면 캡처 도구");
                            key.SetValue("DisplayVersion", "1.0.0");
                            key.SetValue("Publisher", "Custom Tools");
                            key.SetValue("InstallLocation", Path.GetDirectoryName(exePath));
                            key.SetValue("UninstallString", $"\"{exePath}\" /uninstall");
                            key.SetValue("DisplayIcon", File.Exists(iconPath) ? iconPath : exePath);
                            key.SetValue("NoModify", 1);
                            key.SetValue("NoRepair", 1);

                            // 파일 크기 계산
                            if (File.Exists(exePath))
                            {
                                FileInfo fileInfo = new FileInfo(exePath);
                                key.SetValue("EstimatedSize", (int)(fileInfo.Length / 1024)); // KB 단위
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("프로그램 목록 등록 완료");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"프로그램 목록 등록 실패: {ex.Message}");
                }

                // 4. 작업 표시줄 점프 리스트 등록 (Windows 7+)
                try
                {
                    string taskbarKey = $@"Software\Classes\Applications\{executableName}\shell\taskbar";
                    using (var key = Registry.CurrentUser.CreateSubKey(taskbarKey))
                    {
                        if (key != null)
                        {
                            key.SetValue("", "화면 캡처");
                            using (var commandKey = key.CreateSubKey("command"))
                            {
                                if (commandKey != null)
                                {
                                    commandKey.SetValue("", $"\"{exePath}\"");
                                }
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("작업 표시줄 점프 리스트 등록 완료");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"점프 리스트 등록 실패: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine("레지스트리에 모든 아이콘 정보 등록 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"레지스트리 등록 중 오류: {ex.Message}");
            }
        }

        // 레지스트리 정보 제거 (프로그램 삭제 시 사용)
        public void UnregisterFromRegistry()
        {
            try
            {
                string executableName = Path.GetFileName(Application.ExecutablePath);
                string appName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);

                // HKEY_CLASSES_ROOT 정리
                try
                {
                    Registry.ClassesRoot.DeleteSubKeyTree($"Applications\\{executableName}", false);
                }
                catch { }

                // HKEY_CURRENT_USER 정리
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\Applications\{executableName}", false);
                    Registry.CurrentUser.DeleteSubKeyTree($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{appName}", false);
                }
                catch { }

                System.Diagnostics.Debug.WriteLine("레지스트리 정보 제거 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"레지스트리 정보 제거 실패: {ex.Message}");
            }
        }


        private void SetProgramIcon()
        {
            try
            {
                // 여러 크기의 아이콘을 생성하여 작업 표시줄과 창 모두에서 잘 보이도록 함
                Icon customIcon = CreateMultiSizeIcon();
                this.Icon = customIcon;
            }
            catch (Exception ex)
            {
                // 아이콘 생성 실패 시 기본 아이콘 사용
                System.Diagnostics.Debug.WriteLine($"아이콘 생성 실패: {ex.Message}");
            }
        }

        private Icon CreateMultiSizeIcon()
        {
            // 여러 크기의 아이콘 생성 (16x16, 32x32, 48x48)
            var iconSizes = new int[] { 16, 32, 48 };
            var iconImages = new List<Bitmap>();

            foreach (int size in iconSizes)
            {
                Bitmap iconBitmap = CreateCameraIcon(size);
                iconImages.Add(iconBitmap);
            }

            // 여러 크기를 포함하는 아이콘 생성
            return CreateIconFromBitmaps(iconImages);
        }

        private Bitmap CreateCameraIcon(int size)
        {
            Bitmap iconBitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(iconBitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 크기에 맞게 스케일링
                float scale = size / 32.0f;

                // 카메라 본체 (어두운 회색)
                using (SolidBrush bodyBrush = new SolidBrush(Color.FromArgb(64, 64, 64)))
                {
                    g.FillRoundedRectangle(bodyBrush,
                        (int)(4 * scale), (int)(10 * scale),
                        (int)(24 * scale), (int)(16 * scale),
                        Math.Max(1, (int)(3 * scale)));
                }

                // 카메라 렌즈 (검은색 테두리)
                using (SolidBrush lensBrush = new SolidBrush(Color.Black))
                {
                    g.FillEllipse(lensBrush,
                        (int)(8 * scale), (int)(14 * scale),
                        (int)(8 * scale), (int)(8 * scale));
                }

                // 카메라 렌즈 (파란색 내부)
                using (SolidBrush lensInnerBrush = new SolidBrush(Color.FromArgb(0, 120, 215)))
                {
                    g.FillEllipse(lensInnerBrush,
                        (int)(10 * scale), (int)(16 * scale),
                        (int)(4 * scale), (int)(4 * scale));
                }

                // 카메라 플래시 (노란색)
                using (SolidBrush flashBrush = new SolidBrush(Color.Yellow))
                {
                    g.FillEllipse(flashBrush,
                        (int)(20 * scale), (int)(12 * scale),
                        Math.Max(1, (int)(3 * scale)), Math.Max(1, (int)(3 * scale)));
                }

                // 카메라 뷰파인더 (회색)
                using (SolidBrush viewfinderBrush = new SolidBrush(Color.Gray))
                {
                    g.FillRectangle(viewfinderBrush,
                        (int)(10 * scale), (int)(6 * scale),
                        (int)(6 * scale), Math.Max(1, (int)(3 * scale)));
                }
            }
            return iconBitmap;
        }

        private Icon CreateIconFromBitmaps(List<Bitmap> bitmaps)
        {
            // 메모리 스트림에 ICO 파일 형식으로 작성
            using (MemoryStream ms = new MemoryStream())
            {
                // ICO 헤더 작성
                ms.Write(new byte[] { 0, 0 }, 0, 2); // Reserved
                ms.Write(new byte[] { 1, 0 }, 0, 2); // Type (1 = ICO)
                ms.Write(BitConverter.GetBytes((short)bitmaps.Count), 0, 2); // Count

                // 각 이미지의 디렉토리 엔트리를 위한 공간 예약
                long directoryOffset = ms.Position;
                ms.Seek(6 + (16 * bitmaps.Count), SeekOrigin.Begin);

                List<byte[]> imageData = new List<byte[]>();
                List<uint> imageSizes = new List<uint>();

                // 각 비트맵을 PNG로 변환하여 저장
                foreach (Bitmap bitmap in bitmaps)
                {
                    using (MemoryStream pngStream = new MemoryStream())
                    {
                        bitmap.Save(pngStream, ImageFormat.Png);
                        byte[] pngData = pngStream.ToArray();
                        imageData.Add(pngData);
                        imageSizes.Add((uint)pngData.Length);

                        ms.Write(pngData, 0, pngData.Length);
                    }
                }

                // 디렉토리 엔트리 작성
                ms.Seek(directoryOffset, SeekOrigin.Begin);
                uint imageOffset = (uint)(6 + (16 * bitmaps.Count));

                for (int i = 0; i < bitmaps.Count; i++)
                {
                    ms.WriteByte((byte)bitmaps[i].Width);
                    ms.WriteByte((byte)bitmaps[i].Height);
                    ms.WriteByte(0); // Color count
                    ms.WriteByte(0); // Reserved
                    ms.Write(BitConverter.GetBytes((short)1), 0, 2); // Planes
                    ms.Write(BitConverter.GetBytes((short)32), 0, 2); // Bits per pixel
                    ms.Write(BitConverter.GetBytes(imageSizes[i]), 0, 4); // Size
                    ms.Write(BitConverter.GetBytes(imageOffset), 0, 4); // Offset

                    imageOffset += imageSizes[i];
                }

                ms.Seek(0, SeekOrigin.Begin);
                return new Icon(ms);
            }
        }


        private void SetCustomTitleBarColor()
        {
            // Windows 10/11에서 다크 타이틀바 적용
            if (Environment.OSVersion.Version.Major >= 10)
            {
                var attribute = 19; // DWMWA_USE_IMMERSIVE_DARK_MODE
                var useImmersiveDarkMode = 1;
                DwmSetWindowAttribute(this.Handle, (uint)attribute, ref useImmersiveDarkMode, sizeof(int));
            }
        }

        private void SetupSaveDirectory()
        {
            saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ScreenCaptures");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            lblSavedLocation.Text = $"{saveDirectory}";
        }

        private void RegisterGlobalHotkey()
        {
            // F12 키를 전역 핫키로 등록
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                CaptureSelectedArea();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F12)
            {
                CaptureSelectedArea();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SelectAreaButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            overlayForm = new OverlayForm();
            overlayForm.AreaSelected += OverlayForm_AreaSelected;
            overlayForm.ShowDialog();
        }

        private void OverlayForm_AreaSelected(Rectangle area)
        {
            selectedArea = area;
            this.Show();
            this.BringToFront();
            tbMessage.Text = $"Select Area : {area.Width}x{area.Height} (Pos: {area.Location.X}, {area.Location.Y})";
            tbMessage.ForeColor = Color.Green;
            //MessageBox.Show($"영역이 선택되었습니다: {area.Width}x{area.Height}\nF12 키를 눌러 캡처하세요.",
            //               "영역 선택 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CaptureSelectedArea()
        {
            if (selectedArea.IsEmpty)
            {
                tbMessage.Text = "Please Select Capture Area";
                tbMessage.ForeColor = Color.OrangeRed;
                //MessageBox.Show("먼저 캡처할 영역을 선택해주세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (Bitmap bitmap = new Bitmap(selectedArea.Width, selectedArea.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(selectedArea.Location, Point.Empty, selectedArea.Size);
                    }

                    string fileName = $"Capture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    string filePath = Path.Combine(saveDirectory, fileName);
                    bitmap.Save(filePath, ImageFormat.Png);

                    captureCount++;
                    tbTotalCapturedCnt.Text = captureCount.ToString();
                    tbMessage.Text = $"Image Captured!";
                    tbMessage.ForeColor = Color.Blue;
                    //MessageBox.Show($"캡처가 완료되었습니다!\n저장 경로: {filePath}",
                    //               "캡처 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                tbMessage.Text = $"Error : {ex.Message}";
                tbMessage.ForeColor = Color.Red;
                //MessageBox.Show($"캡처 중 오류가 발생했습니다: {ex.Message}",
                //               "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangeSaveLocationButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "캡처 이미지를 저장할 폴더를 선택하세요";
                folderDialog.SelectedPath = saveDirectory;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    saveDirectory = folderDialog.SelectedPath;

                    lblSavedLocation.Text = $"{saveDirectory}";
                    // 저장 위치 라벨 업데이트
                    //Label saveLocationLabel = this.Controls.Find("saveLocationLabel", false)[0] as Label;
                    //if (saveLocationLabel != null)
                    //{
                    //    saveLocationLabel.Text = $"저장 위치: {saveDirectory}";
                    //}
                    tbMessage.Text = $"저장 위치가 변경되었습니다";
                    tbMessage.ForeColor = Color.Green;
                    //MessageBox.Show($"저장 위치가 변경되었습니다:\n{saveDirectory}",
                    //               "저장 위치 변경", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnOpenFileLocation_Click(object sender, EventArgs e)
        {
            try
            {
                if (Directory.Exists(saveDirectory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", saveDirectory);
                }
                else
                {
                    MessageBox.Show("저장 폴더가 존재하지 않습니다.", "오류",
                                   MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폴더를 열 수 없습니다: {ex.Message}", "오류",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
