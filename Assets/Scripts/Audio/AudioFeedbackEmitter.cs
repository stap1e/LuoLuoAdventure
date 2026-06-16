using UnityEngine;

namespace LuoLuoTrip.Audio
{
    public class AudioFeedbackEmitter : MonoBehaviour
    {
        public void Emit(AudioEventId id)
        {
            AudioFeedbackService.Play(id, transform.position);
        }

        public void EmitUI(AudioEventId id)
        {
            AudioFeedbackService.PlayUI(id);
        }
    }
}
