﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MaterialSkin.Animations;

namespace MaterialSkin.Controls
{
    public class MaterialFlatButton : Button, IMaterialControl
    {
        public int Depth { get; set; }
        public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
        public MouseState MouseState { get; set; }
        public bool Primary { get; set; }

        private readonly AnimationManager animationManager;
        private readonly AnimationManager pressAnimationManager;

        public MaterialFlatButton()
        {
            Primary = true;

            animationManager = new AnimationManager(false)
            {
                Increment = 0.03,
                AnimationType = AnimationType.Linear,
            };
            pressAnimationManager = new AnimationManager()
            {
                Increment = 0.03,
                AnimationType = AnimationType.Linear,
            };
            animationManager.OnAnimationProgress += sender => Invalidate();
            pressAnimationManager.OnAnimationProgress += sender => Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            g.Clear(Parent.BackColor);
            if (MouseState != MouseState.OUT )
            {
                    g.FillRectangle(SkinManager.GetFlatButtonHoverBackgroundBrush(), ClientRectangle);
            }

            using (Brush b = new SolidBrush(Color.FromArgb((int)(pressAnimationManager.GetProgress() * 40.PercentageToColorComponent()), SkinManager.GetFlatButtonPressedBackgroundColor().RemoveAlpha()))) 
                g.FillRectangle(b, ClientRectangle);

            if (animationManager.IsAnimating())
            {
                for (int i = 0; i < animationManager.GetAnimationCount(); i++)
                {
                    var animationValue = animationManager.GetProgress(i);
                    var animationSource = animationManager.GetSource(i);
                    using (Brush rippleBrush = new SolidBrush(Color.FromArgb((int)(51 - (animationValue * 50)), Color.Black)))
                    {
                        var rippleSize = (int)(animationValue * Width * 2);
                        g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                    }
                }
            }
            g.DrawString(Text.ToUpper(), SkinManager.ROBOTO_MEDIUM_10, Enabled ? (Primary ? SkinManager.ColorPair.PrimaryBrush : SkinManager.GetMainTextBrush()) : SkinManager.GetFlatButtonDisabledTextBrush(), ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode) return;

            MouseState = MouseState.OUT;
            MouseEnter += (sender, args) =>
            {
                MouseState = MouseState.HOVER;
                Invalidate();
            };
            MouseLeave += (sender, args) => 
            { 
                MouseState = MouseState.OUT;
                Invalidate();
            };
            MouseDown += (sender, args) =>
            {
                MouseState = MouseState.DOWN;

                if (args.Button == MouseButtons.Left)
                {
                    pressAnimationManager.StartNewAnimation(AnimationDirection.In);
                    Invalidate();
                }
            };
            MouseUp += (sender, args) =>
            {
                MouseState = MouseState.HOVER;

                if (args.Button == MouseButtons.Left)
                {
                    animationManager.StartNewAnimation(AnimationDirection.In, args.Location);
                    pressAnimationManager.StartNewAnimation(AnimationDirection.Out);
                }
                Invalidate();
            };
        }
    }
}
