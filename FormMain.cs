using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace UVTileDiscardMapper
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

		private List<Tile> tileControls = new List<Tile>();

		private struct Tile
		{
			public int x;
			public int y;
			public TextBox textBox;
			public Panel panel;
			public Label label;
			public PictureBox pictureBox;
		}

		private void FormMain_Load(object sender, EventArgs e)
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

			// Get panels
			foreach (Control c in tableLayoutPanel.Controls)
			{
				if (c is Panel)
				{
					tileControls.Add(new Tile
					{
						textBox = null,
						panel = (Panel)c,
						x = -1,
						y = -1
					});
				}
			}

			for (int i = 0; i < tileControls.Count; i++)
			{
				foreach (Control c in tileControls[i].panel.Controls)
				{
					// Get coordinates for tiles
					if (c is Label)
					{
						Label label = (Label)c;

						int x = int.Parse(label.Name[label.Name.Length - 1].ToString());
						int y = int.Parse(label.Name[label.Name.Length - 2].ToString());

						tileControls[i] = new Tile
						{
							panel = tileControls[i].panel,
							textBox = tileControls[i].textBox,
							x = x,
							y = y,
							label = label,
							pictureBox = tileControls[i].pictureBox
						};
					}

					// Get textbox and bind events
					if (c is TextBox)
					{
						((TextBox)c).GotFocus += textBoxFocused;
						((TextBox)c).LostFocus += textBoxLostFocus;

						tileControls[i] = new Tile
						{
							panel = tileControls[i].panel,
							textBox = (TextBox)c,
							x = tileControls[i].x,
							y = tileControls[i].y,
							label = tileControls[i].label,
							pictureBox = tileControls[i].pictureBox
						};
					}

					// Get picturebox and bind events
					if (c is PictureBox)
					{
						((PictureBox)c).MouseDoubleClick += PictureBox_MouseDoubleClick;
						((PictureBox)c).MouseClick += PictureBox_MouseClick;

						tileControls[i] = new Tile
						{
							panel = tileControls[i].panel,
							textBox = tileControls[i].textBox,
							x = tileControls[i].x,
							y = tileControls[i].y,
							label = tileControls[i].label,
							pictureBox = (PictureBox)c
						};
					}
				}
			}

			// Reverse the list to follow
			// the TabIndex property's order
			tileControls.Reverse();
		}

        private void FormMain_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
				PasteImage(ActiveControl);
		}

		#region TextBox events
		private void textBoxFocused(object sender, EventArgs e)
		{
			ChangeCoordinateLabelColor(sender, Color.Yellow);
		}

		private void textBoxLostFocus(object sender, EventArgs e)
		{
			ChangeCoordinateLabelColor(sender, Color.White);
		}
		#endregion TextBox events

		#region PictureBox events
		private void PictureBox_MouseClick(object sender, MouseEventArgs e)
		{
            if (e.Button == MouseButtons.Right)
            {
                if (sender != null)
                {
                    if (sender is PictureBox)
                        ((PictureBox)sender).Image = null;
                }
            }
        }

        private void PictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				PasteImage(sender);
		}
        #endregion PictureBox events

        #region Save Button
        private void buttonSaveTileMap_Click(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "Portable Network Graphics | *.png";
			sfd.ShowDialog();

			if (sfd.FileName == "")
				return;

			string MeshName = textBoxMeshName.Text.Trim();
			List<(string MeshText, string Coordinate, Image MeshPicture)> texts = new List<(string, string, Image)>();

			for (int i = 0; i < tileControls.Count; i++)
			{
				if (tileControls[i].textBox != null)
					texts.Add((tileControls[i].textBox.Text.Trim(), "yx(" + tileControls[i].y + "," + tileControls[i].x + ")", tileControls[i].pictureBox.Image));
			}

			CreateImage(MeshName, texts, Path.GetFullPath(sfd.FileName));
		}

		private void buttonSaveTileMap_Enter(object sender, EventArgs e)
        {
			buttonSaveTileMap.BackColor = Color.DodgerBlue;
		}

        private void buttonSaveTileMap_Leave(object sender, EventArgs e)
        {
			buttonSaveTileMap.BackColor = Color.Transparent;
		}
        #endregion Save Button

        #region Clear Button
        private void buttonClear_Click(object sender, EventArgs e)
        {
			DialogResult dr = MessageBox.Show("Clear", "Do you want to clear the form?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (dr != DialogResult.Yes)
				return;

			textBoxMeshName.Text = "";

			for (int i = 0; i < tileControls.Count; i++)
            {
				if (tileControls[i].textBox != null)
					tileControls[i].textBox.Text = "";

				if (tileControls[i].pictureBox != null)
					tileControls[i].pictureBox.Image = null;
            }
        }

        private void buttonClear_Enter(object sender, EventArgs e)
        {
			buttonClear.BackColor = Color.Red;
        }

        private void buttonClear_Leave(object sender, EventArgs e)
        {
			buttonClear.BackColor = Color.Transparent;
		}
		#endregion Clear Button

		// Change coordinate label text, based on the focused textbox
		private void ChangeCoordinateLabelColor(object sender, Color color)
		{
			bool colorChanged = false;

			for (int i = 0; i < tileControls.Count; i++)
			{
				if (tileControls[i].textBox != null)
				{
					if (tileControls[i].textBox == (TextBox)sender)
					{
						if (tileControls[i].label != null)
						{
							tileControls[i].label.ForeColor = color;
							colorChanged = true;
							break;
						}
					}
				}

				if (colorChanged)
					break;
			}
		}

		private void PasteImage(object sender)
		{
			Control activeControl = null;

			// Try to use sender as control
			if (sender is Control)
				activeControl = (Control)sender;

			// If failed, try to use the active control
			if (activeControl == null)
				activeControl = ActiveControl;

			// If also failed, return
			if (activeControl == null)
				return;

			bool correctControlType = false;

			// Check control type
			if (activeControl is TextBox || activeControl is PictureBox)
				correctControlType = true;

			if (!correctControlType)
				return;

			if (!Clipboard.ContainsImage())
				return;

			for (int i = 0; i < tileControls.Count; i++)
			{
				// TextBox is focused
				if (activeControl is TextBox)
				{
					if (tileControls[i].textBox != null)
					{
						if ((TextBox)activeControl == tileControls[i].textBox)
						{
							try
							{
								Image clipboardImage = Clipboard.GetImage();

								if (tileControls[i].pictureBox != null)
									tileControls[i].pictureBox.Image = clipboardImage;
							}
							catch (Exception)
							{
								MessageBox.Show("Error", "Couldn't paste image", MessageBoxButtons.OK, MessageBoxIcon.Hand);
							}
						}
					}
				}

				// PictureBox is focused
				if (activeControl is PictureBox)
				{
					if (tileControls[i].pictureBox != null)
					{
						if ((PictureBox)activeControl == tileControls[i].pictureBox)
						{
							try
							{
								Image clipboardImage = Clipboard.GetImage();
								tileControls[i].pictureBox.Image = clipboardImage;
							}
							catch (Exception)
							{
								MessageBox.Show("Error", "Couldn't paste image", MessageBoxButtons.OK, MessageBoxIcon.Hand);
							}
						}
					}
				}
			}
		}

		// Create and save the image
		private static void CreateImage(string topText, List<(string centerText, string bottomLeftText, Image cellImage)> tableData, string outputPath)
		{
			// Picture resolution
			int width = 2048;
			int height = 2048;

			// Create a new bitmap image with the specified size
			using (Bitmap bitmap = new Bitmap(width, height))
			{
				// Create a graphics object from the bitmap
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					// Clear the background with white color
					graphics.Clear(Color.White);

					// Define the font and brush for the top center text
					Font topFont = new Font("Arial", 24f);
					Brush topBrush = Brushes.Black;

					// Measure the size of the top text
					SizeF topTextSize = graphics.MeasureString(topText, topFont);
					float topTextX = (width - topTextSize.Width) / 2f;
					float topTextY = 20f;

					// Draw the top center text
					graphics.DrawString(topText, topFont, topBrush, topTextX, topTextY);

					// Define the table size and padding
					Font centerTextFont = new Font("Arial", 16f);
					Brush centerTextBrush = Brushes.White;
					Brush centerTextShadowBrush = Brushes.Black;
					Font bottomLeftTextFont = new Font("Arial", 12f);
					Brush bottomLeftTextBrush = Brushes.White;
					Brush bottomLeftShadowBrush = Brushes.Black;
					int rows = 4;
					int cols = 4;
					float padding = 10f; // Padding around the table
					float tableTopY = topTextY + topTextSize.Height + 40f; // 40 pixels below the top text
					float tableHeight = height - tableTopY - padding * 2f;
					float tableWidth = width - padding * 2f;
					float cellWidth = tableWidth / cols;
					float cellHeight = tableHeight / rows;

					// Draw the table
					int dataIndex = 0;
					for (int row = 0; row < rows; row++)
					{
						for (int col = 0; col < cols; col++)
						{
							// Calculate the position of the cell
							float cellX = padding + col * cellWidth;
							float cellY = tableTopY + row * cellHeight;

							// Get the text and image for the current cell
							(string centerText, string bottomLeftText, Image cellImage) cellData = ((dataIndex < tableData.Count) ? tableData[dataIndex] : ("", "", null));
							dataIndex++;

							// Draw the image if present, maintaining the aspect ratio and scaling up to fit within the cell
							if (cellData.cellImage != null)
							{
								float imageWidth = cellData.cellImage.Width;
								float imageHeight = cellData.cellImage.Height;
								float aspectRatio = imageWidth / imageHeight;

								if (aspectRatio > 1f) // Image is wider than tall
								{
									imageWidth = cellWidth;
									imageHeight = cellWidth / aspectRatio;
									if (imageHeight > cellHeight)
									{
										imageHeight = cellHeight;
										imageWidth = cellHeight * aspectRatio;
									}
								}
								else // Image is taller than wide or square
								{
									imageHeight = cellHeight;
									imageWidth = cellHeight * aspectRatio;
									if (imageWidth > cellWidth)
									{
										imageWidth = cellWidth;
										imageHeight = cellWidth / aspectRatio;
									}
								}

								float imageX = cellX + (cellWidth - imageWidth) / 2f;
								float imageY = cellY + (cellHeight - imageHeight) / 2f;
								graphics.DrawImage(cellData.cellImage, imageX, imageY, imageWidth, imageHeight);
							}

							// Define the text format for center alignment and word wrapping
							StringFormat centerTextFormat = new StringFormat
							{
								Alignment = StringAlignment.Center,
								LineAlignment = StringAlignment.Center,
								FormatFlags = StringFormatFlags.LineLimit,
								Trimming = StringTrimming.EllipsisWord
							};

							// Measure the size of the center text
							SizeF centerTextSize = graphics.MeasureString(cellData.centerText, centerTextFont);
							float centerTextX = cellX + (cellWidth - centerTextSize.Width) / 2f;
							float centerTextY = cellY + (cellHeight - centerTextSize.Height) / 2f;

							// Define the text area for center text
							RectangleF centerTextRect = new RectangleF(cellX, cellY, cellWidth, cellHeight);

							// Draw text with white color on top if drawing over an image
							// Otherwise draw it the other way around for better visibility
							if (cellData.cellImage != null)
							{
								// Draw the center text shadow
								graphics.DrawString(cellData.centerText, centerTextFont, centerTextShadowBrush, new RectangleF(centerTextRect.X + 1, centerTextRect.Y + 1, centerTextRect.Width, centerTextRect.Height), centerTextFormat);

								// Draw the center text
								graphics.DrawString(cellData.centerText, centerTextFont, centerTextBrush, centerTextRect, centerTextFormat);
							}
							else
							{
								// Draw the center text shadow
								graphics.DrawString(cellData.centerText, centerTextFont, centerTextBrush, new RectangleF(centerTextRect.X + 1, centerTextRect.Y + 1, centerTextRect.Width, centerTextRect.Height), centerTextFormat);

								// Draw the center text
								graphics.DrawString(cellData.centerText, centerTextFont, centerTextShadowBrush, centerTextRect, centerTextFormat);
							}

							// Define the text format for bottom-left alignment
							StringFormat bottomLeftFormat = new StringFormat
							{
								Alignment = StringAlignment.Near,
								LineAlignment = StringAlignment.Far
							};

							// Measure the size of the bottom-left text
							SizeF bottomLeftTextSize = graphics.MeasureString(cellData.bottomLeftText, bottomLeftTextFont);
							float bottomLeftTextX = cellX + 5; // Padding of 5 pixels from left
							float bottomLeftTextY = cellY + cellHeight - bottomLeftTextSize.Height - 5; // Padding of 5 pixels from bottom

							// Draw the bottom-left text shadow
							graphics.DrawString(cellData.bottomLeftText, bottomLeftTextFont, bottomLeftShadowBrush, bottomLeftTextX + 1, bottomLeftTextY + 1);

							// Draw the bottom-left text
							graphics.DrawString(cellData.bottomLeftText, bottomLeftTextFont, bottomLeftTextBrush, bottomLeftTextX, bottomLeftTextY);

							// Draw cell borders
							graphics.DrawRectangle(Pens.Black, cellX, cellY, cellWidth, cellHeight);
						}
					}
				}

				try
				{
					// Save the image to a file
					bitmap.Save(outputPath);
				}
				catch (Exception)
				{
					MessageBox.Show("Error", "Couldn't save the image", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}
	}
}
