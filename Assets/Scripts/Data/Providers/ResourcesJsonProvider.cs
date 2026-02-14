using UnityEngine;

namespace Data
{
    /// <summary>
    /// Lê JSON da pasta Resources usando Resources.Load&lt;TextAsset&gt;().
    /// Funciona em todas as plataformas (Android, iOS, Desktop, WebGL).
    ///
    /// Os arquivos devem estar em Assets/Resources/{subFolder}/.
    /// Exemplo: Assets/Resources/Data/CompoundsData.json
    ///   → subFolder = "Data"
    ///   → LoadText("CompoundsData.json") → Resources.Load("Data/CompoundsData")
    /// </summary>
    public class ResourcesJsonProvider : IJsonProvider
    {
        private readonly string _subFolder;

        /// <param name="subFolder">
        /// Sub-pasta dentro de Resources (ex: "Data").
        /// </param>
        public ResourcesJsonProvider(string subFolder)
        {
            _subFolder = subFolder;
        }

        public string LoadText(string relativePath)
        {
            // Resources.Load não aceita extensão de arquivo
            string pathWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(relativePath);
            string resourcePath = string.IsNullOrEmpty(_subFolder)
                ? pathWithoutExtension
                : $"{_subFolder}/{pathWithoutExtension}";

            var textAsset = Resources.Load<TextAsset>(resourcePath);

            if (textAsset == null)
            {
                Debug.LogError($"[ResourcesJsonProvider] Arquivo não encontrado em Resources: '{resourcePath}'. " +
                               $"Verifique se o arquivo está em Assets/Resources/{resourcePath}.json");
                return "[]"; // retorna array vazio para evitar crash
            }

            return textAsset.text;
        }
    }
}
