namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// Describes the current playback status.
    /// </summary>
    public enum ArPlaybackStatus
    {
        /// <summary>
        /// The session is not playing back an MP4 dataset file.
        /// </summary>
        None = 0,

        /// <summary>
        /// Playback is in process without issues.
        /// </summary>
        Ok = 1,

        /// <summary>
        /// Playback has stopped due to an error.
        /// </summary>
        IOError = 2,

        /// <summary>
        /// Playback has finished successfully.
        /// </summary>
        Finished = 3,
    };

    /// <summary>
    /// Extension methods for the <see cref="ArPlaybackStatus"/> enum.
    /// </summary>
    public static class ArPlaybackStatusExtensions
    {
        /// <summary>
        /// Determines whether a recorded session is currently playing.
        /// </summary>
        /// <param name="status">The <see cref="ArPlaybackStatus"/> being extended.</param>
        /// <returns>Returns `true` if the <paramref name="status"/> is <see cref="ArPlaybackStatus.Ok"/>. Returns
        ///     `false` otherwise.</returns>
        public static bool Playing(this ArPlaybackStatus status) => status == ArPlaybackStatus.Ok;
    }
}
