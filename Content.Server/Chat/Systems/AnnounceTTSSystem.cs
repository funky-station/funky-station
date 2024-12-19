using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using Robust.Shared.Timing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio.Components;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.Player;
using Content.Server.Communications;
using Content.Shared.Communications;
using Content.Shared.Silicons.StationAi;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.Chat.Systems;


/// <summary>
///     AnnounceTTSSystem is responsible for TTS announcements, such as through the Station AI.
/// </summary>
public sealed partial class AnnounceTTSSystem : EntitySystem
{
	[Dependency] private readonly SharedAudioSystem _audio = default!;
	[Dependency] private readonly ITimerManager _timerManager = default!;
	[Dependency] private readonly IEntityManager _entityManager = default!;

	private Queue<string> ttsQueue = default!;
	private CancellationTokenSource source = default!;
	private const int defaultDurationMs = 100;
	private const int bufferBetweenMessages = 10;

	public override void Initialize()
    {
        base.Initialize();

		ttsQueue = new Queue<string>();
		source = new();

		var dependencies = IoCManager.Instance!;

		Timer.Spawn(defaultDurationMs, PopTTSMessage, source.Token);
	}

	private void PopTTSMessage()
	{
		(EntityUid Entity, AudioComponent Component)? res = null;
		bool played = false;
		string filepath = "";
		if (ttsQueue.Count > 0)
		{
			string dequeued = ttsQueue.Dequeue();
			filepath = (dequeued.StartsWith("/") ? dequeued : ("/Audio/Announcements/VoxFem/"+dequeued+".ogg"));
			try
			{
				res = _audio.PlayGlobal(filepath, Filter.Broadcast(), true, AudioParams.Default.WithVolume(-2f));
				played = true;
			}
			catch (Exception e)
			{
				played = false;
			}				
		}
		else
		{
			played = false;
		}

		if (played == true)
			Timer.Spawn((int)(_audio.GetAudioLength(filepath).TotalMilliseconds) + bufferBetweenMessages, PopTTSMessage, source.Token);
		else
			Timer.Spawn(defaultDurationMs, PopTTSMessage, source.Token);
	}

	public void QueueTTSMessage(List<string> filenames, string? alertSound = null)
	{
		if (alertSound != null)
			ttsQueue.Enqueue(alertSound);
		foreach (string filename in filenames)
		{
			ttsQueue.Enqueue(filename);
		}
		
	}

	public bool CanTTS(EntityUid user)
	{
		return _entityManager.HasComponent<StationAiOverlayComponent>(user);
	}

	public static List<string> PrepareTTSMessage(string msg)
	{
		string lowered = msg.ToLower();
		lowered = lowered.Replace("&", " and ");
		lowered = lowered.Replace("+", " plus ");
		lowered = lowered.Replace("/", " or ");
		lowered = lowered.Replace(".", " . ");
		lowered = lowered.Replace(",", " , ");
		lowered = lowered.Replace("%", " percent ");
		string[] splitted = lowered.Split(' ', '\n', '-', ';', '+', '/', '?', '!', '&', '*', '(', ')', '%', '$', '[', ']');
		return new List<string>(splitted);
	}
}
