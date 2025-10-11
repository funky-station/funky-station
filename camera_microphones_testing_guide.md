# Camera Microphones Testing Guide

This guide explains how to test the Malf AI Camera Microphones upgrade functionality without requiring multiple clients.

## Prerequisites

1. Admin privileges (you need at least AdminFlags.Admin for OSay command)
2. A working station with surveillance cameras
3. A Malf AI with the Camera Microphones upgrade purchased and enabled

## Testing Setup

### Step 1: Become or Spawn a Malf AI
1. Use admin commands to become a Malf AI or spawn one
2. Purchase the "Camera Microphones" upgrade from the Malf AI store (costs 3 CPU)
3. Toggle the upgrade ON using the action button

### Step 2: Position the AI Eye
1. As the AI, move your camera view near a surveillance camera
2. Make sure you're within 5 tiles of the camera (this is configurable in MalfAiCameraMicrophonesComponent.RadiusTiles)

### Step 3: Test IC Chat Reception

#### Method 1: Using OSay Command
The OSay admin command can force any entity to speak IC chat, which will trigger the camera microphones system.

**Command syntax:**
```
osay <entity_uid> <chat_type> <message>
```

**Chat types available:**
- Say (normal speech)
- Whisper (reduced range)
- Shout (increased range)

**Example usage:**
1. Find an entity near a camera (use entity spawn or find existing entities)
2. Get the entity's UID (you can see this in entity examine or admin tools)
3. Run: `osay 1234 Say "Testing camera microphones"`
4. The AI should receive this message if:
   - The speaking entity is within voice range of a camera with microphones
   - The AI eye is within 5 tiles of that same camera
   - The Camera Microphones upgrade is enabled

#### Method 2: Ghost Possession
1. Go into ghost/observer mode
2. Possess an entity near a surveillance camera
3. Speak normally using the say command
4. Switch back to AI to see if you received the message

### Step 4: Verify Functionality

**Expected behavior:**
- When conditions are met, the AI should see IC chat messages in their chat log
- The range shown should be the distance from speaker to camera (not speaker to AI)
- Messages should appear as normal IC chat, not in a separate camera interface

**Troubleshooting:**
- Ensure cameras have both SurveillanceCameraMicrophoneComponent and ActiveListenerComponent
- Verify the camera is Active (cameras can be disabled)
- Check that the AI eye is close enough to the camera (default: 5 tiles)
- Confirm the speaker is within voice range of the camera
- Make sure the upgrade is toggled ON

## Testing Different Scenarios

1. **Range Testing**: Test with speakers at different distances from cameras
2. **Multiple Cameras**: Test with multiple microphone-enabled cameras
3. **Cross-Z-Level**: Test if it works across different floors/levels
4. **Different Chat Types**: Test whispers (shorter range) and shouts (longer range)
5. **Camera States**: Test with active/inactive cameras

## Debug Information

The camera microphones system processes these conditions:
1. Message must have VoiceRange > 0 (IC chat only)
2. AI must have MalfAiCameraMicrophonesComponent with EnabledEffective = true
3. AI must have a valid RemoteEntity (eye)
4. At least one camera must satisfy both:
   - Speaker within voice range of camera
   - AI eye within RadiusTiles of same camera

## Common Issues

- **No microphone cameras**: Not all cameras have microphones by default
- **Upgrade not enabled**: Make sure to toggle the upgrade after purchasing
- **Wrong chat type**: OOC messages won't trigger the system
- **Camera inactive**: Cameras can be disabled/powered off

This testing method allows full verification of the camera microphones functionality using only admin commands and single-client testing.
