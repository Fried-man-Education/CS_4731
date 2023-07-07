/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VehicleBehaviour;

namespace GameAI {
    [RequireComponent(typeof(AIVehicle))]
    [RequireComponent(typeof(AudioSource))]

    public class AIEngineSoundManager : MonoBehaviour {

        [Header("AudioClips")]
        public AudioClip starting;
        public AudioClip rolling;
        public AudioClip stopping;

        [Header("pitch parameter")]
        public float flatoutSpeed = 20.0f;
        [Range(0.0f, 3.0f)]
        public float minPitch = 0.7f;
        [Range(0.0f, 0.1f)]
        public float pitchSpeed = 0.05f;

        private AudioSource _source;
        private AIVehicle _vehicle;
        
        void Start () {
            var audioSources = GetComponents<AudioSource>();

            _source = audioSources[audioSources.Length - 1];
            

            if (_source == null) Debug.Log("no source");

            _vehicle = GetComponent<AIVehicle>();

            if (_vehicle == null) Debug.Log("no vehicle");
        }


        void Update () {
            if (_vehicle.Handbrake && _source.clip == rolling)
            {
                Debug.Log($"Playing stopping. Handbrake {_vehicle.Handbrake} isPlaying: {_source.isPlaying}");
                _source.clip = stopping;
                _source.Play();
            }

            if (!_vehicle.Handbrake && (_source.clip == stopping || _source.clip == null))
            {
                Debug.Log($"Playing starting. Handbrake {_vehicle.Handbrake} isPlaying: {_source.isPlaying}");
                _source.clip = starting;
                _source.Play();

                _source.pitch = 1;
            }

            if (!_vehicle.Handbrake && !_source.isPlaying)
            {

                    //Debug.Log($"Playing rolling again. Handbrake {_vehicle.Handbrake} isPlaying: {_source.isPlaying}");
                    _source.clip = rolling;
                    _source.Play();
                
            }

            if (_source.clip == rolling)
            {
                _source.pitch = Mathf.Lerp(_source.pitch, minPitch + Mathf.Abs(_vehicle.Speed) / flatoutSpeed, pitchSpeed);
            }
        }
    }
}
