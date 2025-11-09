using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.StartScreen;

using Piktosaur.Utils;
using System.Diagnostics;

namespace Piktosaur.Services
{
    public class JumpListHandler
    {
        public static void AddToJumpList(StorageFolder folder)
        {
            _ = _AddToJumpList(folder);
        }

        private static async Task _AddToJumpList(StorageFolder folder)
        {
            try
            {
                var jumpList = await JumpList.LoadCurrentAsync();
                jumpList.SystemGroupKind = JumpListSystemGroupKind.Recent;

                var folderPath = folder.Path;
                var displayName = FileSystem.GetFormattedFolderName(folderPath);
                var folderItem = JumpListItem.CreateWithArguments(folderPath, displayName);

                AddFolderItem(jumpList, folderItem);

                await jumpList.SaveAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                // The JumpList functionality is not critical, so safe to ignore
            }
        }

        private static void AddFolderItem(JumpList jumpList, JumpListItem jumpListItem)
        {
            var existing = jumpList.Items.Where(i => i.Arguments == jumpListItem.Arguments).ToList();
            foreach (var item in existing)
            {
                jumpList.Items.Remove(item);
            }

            jumpList.Items.Add(jumpListItem);

            if (jumpList.Items.Count > 5)
            {
                for (int i = 0; i < jumpList.Items.Count - 5; i++)
                {
                    var item = jumpList.Items[0];
                    jumpList.Items.Remove(item);
                }
            }
        }
    }
}
