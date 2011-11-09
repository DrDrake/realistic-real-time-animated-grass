using System;
using System.Collections.Generic;
using System.Text;

using SlimDX.Direct3D10;

namespace RealtimeGrass.Utility
{
    public enum TextureType 
    {
        TextureTypeDiffuse,
        TextureTypeAlpha,
        TextureTypeCube,
        TextureTypeNormal
    }

    public class TextureFormat : IDisposable
    {
        private readonly string m_filePath;
        private readonly ImageLoadInformation m_loadInfo;
        private readonly TextureType m_type;
        private Texture2D m_texture;
        private ShaderResourceView m_shaderResource;
        private string m_shaderName;

        public TextureFormat(
            string filePath,
            ImageLoadInformation loadInfo,
            TextureType type
        )
        {
            m_filePath = filePath;
            m_loadInfo = loadInfo;
            m_type = type;
            m_texture = null;
            m_shaderResource = null;
        }

        public string FilePath { get { return m_filePath; } private set { } }
        public ImageLoadInformation LoadInfo { get { return m_loadInfo; } private set { } }
        public TextureType Type { get { return m_type; } private set { } }

        public Texture2D Texture { get { return m_texture; } set { m_texture = value; } }
        public ShaderResourceView ShaderResource { get { return m_shaderResource; } set { m_shaderResource = value; } }
        public string ShaderName { get { return m_shaderName; } set { m_shaderName = value; } }

        public void LoadFromFile(Device device)
        {
            if (m_loadInfo != null)
                m_texture = Texture2D.FromFile(device, m_filePath, m_loadInfo);
            else
                m_texture = Texture2D.FromFile(device, m_filePath);
            //For Cubemaps
            if (m_type == TextureType.TextureTypeCube)
            {
                Texture2DDescription textureDesc = m_texture.Description;
                ShaderResourceViewDescription shaderDesc = new ShaderResourceViewDescription();

                shaderDesc.Format = textureDesc.Format;
                shaderDesc.Dimension = ShaderResourceViewDimension.TextureCube;
                shaderDesc.MipLevels = textureDesc.MipLevels;
                shaderDesc.MostDetailedMip = 0;

                m_shaderResource = new ShaderResourceView(device, m_texture, shaderDesc);
            }
            else
            {
                m_shaderResource = new ShaderResourceView(device, m_texture);
            }
            m_texture.Dispose();
        }

        public void Dispose()
        {
            if (! m_texture.Disposed)
                m_texture.Dispose();
        }
    }
}
