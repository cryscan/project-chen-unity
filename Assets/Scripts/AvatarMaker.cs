using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Infrastructure.Editor
{
    public class AvatarMaker
    {
        [MenuItem("Custom Tools/Make Avatar Mask")]
        private static void MakeAvatarMask()
        {
            GameObject activeGameObject = Selection.activeGameObject;

            if (activeGameObject != null)
            {
                AvatarMask avatarMask = new AvatarMask();

                avatarMask.AddTransformPath(activeGameObject.transform);

                var path = string.Format("Assets/{0}.mask", activeGameObject.name.Replace(':', '_'));
                AssetDatabase.CreateAsset(avatarMask, path);
            }
        }

        [MenuItem("Custom Tools/Make Avatar")]
        private static void MakeAvatar()
        {
            GameObject activeGameObject = Selection.activeGameObject;

            if (activeGameObject != null)
            {
                Avatar avatar = AvatarBuilder.BuildGenericAvatar(activeGameObject, "");
                avatar.name = activeGameObject.name;

                var path = string.Format("Assets/{0}.ht", avatar.name.Replace(':', '_'));
                AssetDatabase.CreateAsset(avatar, path);
            }
        }
    }
}
#endif