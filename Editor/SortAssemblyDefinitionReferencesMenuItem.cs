using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;

namespace Kogane.Internal
{
    internal static class SortAssemblyDefinitionReferencesMenuItem
    {
        private const string MENU_ITEM_NAME = "Assets/Kogane/Sort Assembly Definition References";

        private const string MESSAGE = "選択中の AssemblyDefinitionAsset と選択中のフォルダに含まれる AssemblyDefinitionAsset の References を名前順でソート";

        [MenuItem( MENU_ITEM_NAME, false, 1655616629 )]
        private static void Convert()
        {
            if ( !EditorUtility.DisplayDialog( "", $"{MESSAGE}しますか？", "はい", "いいえ" ) ) return;

            var currentDirectory = Directory.GetCurrentDirectory().Replace( "\\", "/" ) + "/";

            var allAssemblyDefinitionAssetPath = AssetDatabase
                    .GetAllAssetPaths()
                    .Where( x => x.EndsWith( ".asmdef" ) )
                    .ToArray()
                ;

            var directoryPathArray = Selection.objects
                    .OfType<DefaultAsset>()
                    .Select( x => AssetDatabase.GetAssetPath( x ) )
                    .Where( x => AssetDatabase.IsValidFolder( x ) )
                    .Select( x => $"{x}/" )
                    .ToArray()
                ;

            var assemblyDefinitionAssetPathInFolderArray = allAssemblyDefinitionAssetPath
                    .Where( x => directoryPathArray.Any( y => x.StartsWith( y ) ) )
                    .ToArray()
                ;

            var assemblyDefinitionAssetArray = Selection.objects
                    .OfType<AssemblyDefinitionAsset>()
                    .Select( x => AssetDatabase.GetAssetPath( x ) )
                    .Concat( assemblyDefinitionAssetPathInFolderArray )
                    .Where( x => IsAssetOrPackage( currentDirectory, x ) )
                    .Distinct()
                    .ToArray()
                ;

            try
            {
                AssetDatabase.StartAssetEditing();

                var length = assemblyDefinitionAssetArray.Length;

                for ( var i = 0; i < length; i++ )
                {
                    var number    = i + 1;
                    var assetPath = assemblyDefinitionAssetArray[ i ];

                    EditorUtility.DisplayProgressBar
                    (
                        title: "Sort Assembly Definition References",
                        info: $"{number} / {length}    {assetPath}",
                        progress: ( float )number / length
                    );

                    AssemblyDefinitionReferencesSorter.Sort( assetPath );
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog( "", $"{MESSAGE}しました", "OK" );
        }

        private static bool IsAssetOrPackage( string currentDirectory, string assetPath )
        {
            var fullPath     = Path.GetFullPath( assetPath ).Replace( "\\", "/" );
            var relativePath = fullPath.Replace( currentDirectory, "" );

            return relativePath.StartsWith( "Assets/" ) ||
                   relativePath.StartsWith( "Packages/" );
        }
    }
}