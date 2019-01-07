﻿using MiniEngine.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace MiniEngine.UI
{
    public sealed class UIState
    {
        private const string UIStateFile = "ui_state.xml";

        public UIState()
        {
            this.SelectedRenderTargets = new List<RenderTargetDescription>(0);
            this.DebugDisplay = DebugDisplay.None;
            this.Columns = 4;
            this.ShowGui = true;
        }

        public bool ShowGui { get; set; }
        public int Columns;
        public DebugDisplay DebugDisplay { get; set; }

        public bool ShowDemo;
        public int SelectedEntity;
        public bool ShowEntityWindow;

        [XmlIgnore]
        public List<RenderTargetDescription> SelectedRenderTargets { get; }

        [XmlIgnore]
        public RenderTargetDescription SelectedRenderTarget { get; set; }             


        [EditorBrowsable(EditorBrowsableState.Never)]
        public List<string> SelectedRenderTargetNames { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string SelectedRenderTargetName { get; set; }

        public void Serialize()
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            this.SelectedRenderTargetName = this.SelectedRenderTarget?.Name;
            this.SelectedRenderTargetNames = this.SelectedRenderTargets.Select(rt => rt.Name).ToList();

            var serializer = new XmlSerializer(typeof(UIState));
            using (var stream = File.CreateText(UIStateFile))
            {
                serializer.Serialize(stream, this);
            }

            Thread.CurrentThread.CurrentCulture = culture;
        }

        public static UIState Deserialize(GBuffer gBuffer)
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                var serializer = new XmlSerializer(typeof(UIState));
                using (var stream = File.OpenRead(UIStateFile))
                {
                    var uiState = (UIState)serializer.Deserialize(stream);

                    uiState.SelectedRenderTarget = gBuffer.RenderTargets.FirstOrDefault(rt => rt.Name.Equals(uiState.SelectedRenderTargetName));
                    foreach (var name in uiState.SelectedRenderTargetNames)
                    {
                        uiState.SelectedRenderTargets.Add(gBuffer.RenderTargets.First(rt => rt.Name.Equals(name)));
                    }

                    return uiState;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error: could not deserialize {UIStateFile}, {e.ToString()}");
                return new UIState();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }            
        }
    }
}