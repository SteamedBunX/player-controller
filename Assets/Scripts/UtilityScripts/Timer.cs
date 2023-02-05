using System.Collections.Generic;
using UnityEngine;

public class Timer {
  Dictionary<string, float> timeStore = new Dictionary<string, float>();
  public void storeTime(string timeKey, float timeValue) {
    timeStore[timeKey] = timeValue;
  }

  public void resetTime(string timeKey) {
    timeStore[timeKey] = 0f;
  }

  public float getTimeSince(string timeKey) {
    return Time.time - (timeStore.ContainsKey(timeKey) ? timeStore[timeKey] : 0f);
  }

  public float getTimeOf(string timeKey) {
    return timeStore.ContainsKey(timeKey) ? timeStore[timeKey] : 0f;
  }
}