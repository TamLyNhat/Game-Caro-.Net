using System;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;

namespace GameCaro
{
    public partial class Form1 : Form
    {
        #region Properties

        private ChessBoardManager ChessBoard;
        SocketManager socket;

        #endregion Properties

        public Form1()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;

            ChessBoard = new ChessBoardManager(pnlChessBoard, txtPlayerName, pctbMark);
            ChessBoard.EndedGame += ChessBoard_EndedGame;
            ChessBoard.PlayerMarked += ChessBoard_PlayerMarked;

            prcbCoolDown.Step = Cons.COOL_DOWN_STEP;
            prcbCoolDown.Maximum = Cons.COOL_DOWN_TIME;
            prcbCoolDown.Value = 0;

            tmCoolDown.Interval = Cons.COOL_DOWN_INTERVAL;

            socket = new SocketManager();

            NewGame();

        }

        private void EndGame()
        {
            tmCoolDown.Stop();
            pnlChessBoard.Enabled = false;
            //MessageBox.Show("Kết thúc");
        }

        private void ChessBoard_PlayerMarked(object sender, ButtonClickEvent e)
        {
            tmCoolDown.Start();
            pnlChessBoard.Enabled = false;
            prcbCoolDown.Value = 0;

            socket.Send(new SocketData((int)SocketCommand.SEND_POINT, "", e.ClickedPoint));

            Listen();
        }

        private void ChessBoard_EndedGame(object sender, EventArgs e)
        {
            EndGame();
            socket.Send(new SocketData((int)SocketCommand.END_GAME, "", new Point()));
        }

        private void tmCoolDown_Tick(object sender, System.EventArgs e)
        {
            prcbCoolDown.PerformStep();

            if (prcbCoolDown.Value >= prcbCoolDown.Maximum)
            {
                EndGame();
                socket.Send(new SocketData((int)SocketCommand.TIME_OUT, "", new Point()));
            }
        }

        private void NewGame()
        {
            prcbCoolDown.Value = 0;
            tmCoolDown.Stop();
            ChessBoard.DrawChessBoard();
        }

        private void Quit()
        {
            Application.Exit();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //newGameToolStripMenuItem.Enabled = false;
            Quit();
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
            socket.Send(new SocketData((int)SocketCommand.NEW_GAME, "", new Point()));

            pnlChessBoard.Enabled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn thoát", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.OK)
            {
                e.Cancel = true;
            }
            else
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.QUIT, "", new Point()));
                }
                catch { }
            }

        }

        private void btnClan_Click(object sender, EventArgs e)
        {
            socket.IP = txtIP.Text;

            if (!socket.ConnectServer())
            {
                btnClan.Enabled = false;
                socket.isServer = true;
                pnlChessBoard.Enabled = true;
                socket.CreateServer();
            }
            else
            {
                socket.isServer = false;
                pnlChessBoard.Enabled = false;
                Listen();
                btnClan.Enabled = false;
            }

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            txtIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);

            if (string.IsNullOrEmpty(txtIP.Text))
            {
                txtIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            }

        }

        void Listen()
        {
            Thread listenThread = new Thread(() =>
            {
                try
                {
                    SocketData data = (SocketData)socket.Receive();

                    ProcessData(data);
                }
                catch (Exception e)
                {
                }
            });
            listenThread.IsBackground = true;
            listenThread.Start();

        }

        private void ProcessData(SocketData data)
        {
            switch (data.Command)
            {
                case (int)SocketCommand.NOTIFY:
                    MessageBox.Show(data.Message);
                    break;
                case (int)SocketCommand.NEW_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        NewGame();
                        pnlChessBoard.Enabled = false;
                    }));
                    break;
                case (int)SocketCommand.SEND_POINT:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        prcbCoolDown.Value = 0;
                        pnlChessBoard.Enabled = true;
                        tmCoolDown.Start();
                        ChessBoard.OtherPlayerMark(data.Point);
                    }));
                    break;
                case (int)SocketCommand.END_GAME:
                    MessageBox.Show("Đã 5 con trên 1 hàng, kết thúc game!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case (int)SocketCommand.TIME_OUT:
                    MessageBox.Show("Hết giờ !!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case (int)SocketCommand.QUIT:
                    tmCoolDown.Stop();
                    newGameToolStripMenuItem.Enabled = false;
                    pnlChessBoard.Enabled = false;
                    MessageBox.Show("Người chơi đã thoát...", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                default:
                    break;
            }


            Listen();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pnlChessBoard.Enabled = false;
            txtLuat.Text = "Bật 2 ứng dụng cùng lúc, mỗi ứng dụng\nbấm vào nút \"kết nối mạng LAN\". " +
                "\nBạn có thời gian 10 giây để suy nghĩ \nnước cờ, sau 10s thì game sẽ kết thúc\n " +
                "Bên nào kết nối trước thì bên đó \nđánh trước";
        }
    }
}