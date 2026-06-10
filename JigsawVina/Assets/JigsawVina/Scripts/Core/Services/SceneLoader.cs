using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace JigsawVina.Core.Services
{
    public class SceneLoader
    {
        public async UniTask LoadSceneAsync(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null) return;
            await op.ToUniTask();
        }
    }
}
