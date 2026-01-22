using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthEat.Exceptions;
using SFML;

namespace HealthEat
{
    /// <summary>
    /// Manages loading and retrieval of game resources, primarily textures.
    /// Implements a singleton pattern.
    /// </summary>
    internal class ResourceManager
    {

        /// <summary>
        /// Initializes a new ResourceManager and sets it as the singleton instance.
        /// </summary>
        public ResourceManager()
        {
            #if HE_DEBUG
            Console.WriteLine("[ResourceManager]: ResourceManager created!");
            #endif

            m_Instance = this;
        }


        ~ResourceManager()
        {
            m_Textures.Clear();
        }


        /// <summary>
        /// Gets loaded texture of name specified by tname.
        /// </summary>
        /// <param name="tname">Name of texture</param>
        /// <returns>Loaded texture</returns>
        /// <exception cref="HE_MissingAssetException">Throws when texture of specified name is not loaded</exception>
        public HE_Texture GetTexture(string tname)
        {
            string fullTname = "resources/" + tname;

            if (!m_Textures.ContainsKey(fullTname))
            {
                throw new HE_MissingAssetException("[ResourceManager]: Asset doesn't exist in ResourceManager resources - ", tname);
            }

            return m_Textures[fullTname];
        }

        /// <summary>
        /// Attempts to load a texture from a file path.
        /// </summary>
        /// <param name="file">The file path to load the texture from.</param>
        /// <returns>True if the texture was successfully loaded, false otherwise.</returns>
        public bool TryLoadTextureFromFile(string file)
        {
            try
            {
                HE_Texture txt = new HE_Texture(file);
                m_Textures.Add(file, txt);
                return true;

            } catch (LoadingFailedException e)
            {
                #if HE_DEBUG
                Console.WriteLine("[ResourceManager]: Failed to load asset from TryLoadTextureFromFile method: " + e.Message);
                #endif
                return false;
            }
        }

        /// <summary>
        /// Loads all texture resources from the resources folder.
        /// </summary>
        /// <exception cref="HE_AssetLoadException">Thrown when the resource folder cannot be found.</exception>
        public void LoadResources()
        {
            if (!Directory.Exists(m_ResourceFoldetPath))
            {
                throw new HE_AssetLoadException("[ResourceManager]: Cannot find resource folder");
            }

            string[] assetFiles = Directory.GetFiles(m_ResourceFoldetPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in assetFiles)
            {
                string assetName = Path.GetFileName(file);
                AddNewTexture($"{m_ResourceFoldetPath}/{assetName}");
            }
        }


        private void AddNewTexture(string name)
        {
            try
            {
                HE_Texture txt = new HE_Texture(name);
                m_Textures.Add(name, txt);

                #if HE_DEBUG
                Console.WriteLine($"[ResourceManager]: Loaded asset - {name}");
                #endif
            }
            catch (LoadingFailedException e)
            {
                #if HE_DEBUG
                Console.WriteLine($"[ResourceManager]: Failed loading asset {name} ({e.Message})");
                #endif
            }
        }

        /// <summary>
        /// Gets the singleton instance of the ResourceManager.
        /// </summary>
        public static ResourceManager Get => m_Instance;

        private static ResourceManager m_Instance = null!;
        private Dictionary<string, HE_Texture> m_Textures = new();
        private readonly string m_ResourceFoldetPath = "resources";

    }
}
