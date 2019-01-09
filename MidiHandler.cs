using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidiHandler : MonoBehaviour {

    public GameObject cube;
    private bool midiPlayed = false;
    private bool alreadyPlayed = false;
    ChuckSubInstance myChuck;
    ChuckFloatSyncer midiPlayedSyncer;
    ChuckFloatSyncer midiValueSyncer;
    ChuckFloatSyncer intensitySyncer;

    // Use this for initialization
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myChuck.RunCode(@"
            global float midiPlayed;
            global float midiValue;
            global float intensity;

            // Midi Set-up
            //---------------------------------------------------------------------
            // number of the device to open (see: chuck --probe)
            0 => int deviceNumber;
            if (me.args()) me.arg(0) => Std.atoi => deviceNumber;
            MidiIn min;
            MidiMsg msg;
            // open the device
            if (!min.open(deviceNumber)) me.exit();
            // print out device that was opened
            <<< ""MIDI device: "", min.num(), ""-> "", min.name() >>>;
            //--------------------------------------------------------------------- 

            // make our own event
                class NoteEvent extends Event
                {
                int note;
                int velocity;
            }

            // the event
            NoteEvent on;
            // array of ugen's handling each note
            Event @ us[128];

            // the base patch
            //Gain g => JCRev r => dac;
            Gain g => JCRev r => LPF lpf => HPF hpf => Pan2 p => dac;
            1.0 => g.gain;
            .6 => r.mix;
            10000 => lpf.freq;
            100 => hpf.freq;

            // handler for a single voice
            fun void handler()
            {
                // don't connect to dac until we need it
                FMVoices m;
                Event off;
                int note;

                while (true)
                {
                    on => now;
                    on.note => note;
                    // dynamically repatch
                    m => g;
                    Std.mtof(note) => m.freq;
                    //Math.random2f( .6, .8 ) => m.pluckPos;
                    on.velocity / 128.0 => m.noteOn;
                    off @=> us[note];

                    off => now;
                    null @=> us[note];
                    m =< g;
                }
            }

            // spork handlers, one for each voice
            for( 0 => int i; i< 20; i++ ) spork ~handler();

            fun void filterTracker() {
                while (true) {
                    // daytime
                    1.0 => g.gain;
                    .6 => r.mix;
                    for ( 0 => int i; i < 11979; i++ ) {
                        8000 - i/3 => lpf.freq;
                        7000 - i/3 => hpf.freq;
                        -.9 => p.pan;
                        1::ms => now;
                    }
                    0 => g.gain;
                    0 => r.mix;
                    20::ms => now;
                    // nighttime
                    1.0 => g.gain;
                    .6 => r.mix;
                    for ( 0 => int i; i < 11979; i++ ) {
                        100 + i/12 => lpf.freq;
                        100 => hpf.freq;
                        .9 => p.pan;
                        1::ms => now;
                    }
                    0 => g.gain;
                    0 => r.mix;
                    20::ms => now;
                }
            }

            3::second => now;

            spork ~filterTracker();

            // infinite time-loop
            while( true )
            {
                // wait on midi event
                min => now;
                
                // get the midimsg
                while(min.recv(msg ) )
                {
                    // catch only noteon
                    if(msg.data1 != 144 )
                        continue;
                    
                    1.0 => midiPlayed;
                    msg.data3 => intensity;
                    <<< ""keyNum down: "", msg.data2 >>>;
                    msg.data2 => int midiDataInt;
                    midiDataInt => float midiDataFloat;
                    midiDataFloat - 60 => midiValue;
                    25::ms => now;

                    // check velocity
                    if(msg.data3 > 0 )
                    {
                        // store midi note number
                        msg.data2 => on.note;
                        // store velocity
                        msg.data3 => on.velocity;
                        // signal the event
                        on.signal();
                        // yield without advancing time to allow shred to run
                        me.yield();
                    }
                    else
                    {
                        if(us[msg.data2] != null ) us[msg.data2].signal();
                    }
                }
                0.0 => midiPlayed;
            }
        ");

        midiPlayedSyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        midiPlayedSyncer.SyncFloat(myChuck, "midiPlayed");
        midiValueSyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        midiValueSyncer.SyncFloat(myChuck, "midiValue");
        intensitySyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        intensitySyncer.SyncFloat(myChuck, "intensity");
    }

    // Update is called once per frame
    void Update () {
        float midiPlayedSyncerValue = midiPlayedSyncer.GetCurrentValue();
        if (midiPlayedSyncerValue > 0.5f) midiPlayed = true;
        if (midiPlayed == true)
        {
            float midiNote = midiValueSyncer.GetCurrentValue();
            GameObject newCube = Instantiate(cube, new Vector3((-4.0f + (0.42f * midiNote)), 10.0f, 15.0f), Quaternion.identity) as GameObject;
            float intensity = intensitySyncer.GetCurrentValue();
            newCube.transform.localScale = new Vector3(0.00002f*intensity, 0.00002f*intensity, 0.00002f*intensity);
            Rigidbody clone = newCube.AddComponent<Rigidbody>();
            midiPlayed = false;
        }
    }
}