﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  public class AGINote
  {
    int mFreqDiv;
    int mDuration;
    byte mAttenuation;
    internal AGISound mSndParent;
    internal AGITrack mTrkParent;
    string strErrSource;
    public byte Attenuation
       { 
      get  
      { 
        return mAttenuation; 
      }  
      set
      {
        //validate
        if (value > 15)
        {
          //invalid item
          throw new Exception("6, strErrSource, Overflow");
        }
        mAttenuation = value;
        //if parent is assigned
        if (mSndParent != null)
        {
          //notify parent
          mSndParent.NoteChanged();
        }
      }
    }
    public int Duration
    {
      get
      {
        return mDuration;
      }
      set
      {
        //validate
        if (value < 0 || value > 0xFFFF)
        {
          //invalid frequency
          throw new Exception("6, strErrSource, Overflow");
        }
        mDuration = value;
        //notify parents, if applicable
        mSndParent?.NoteChanged();
        mTrkParent?.SetLengthDirty();
      }
    }
    public int FreqDivisor
    {
      get
      {
        return mFreqDiv;
      }
      set
      {
        //validate
        if (value < 0 || value > 1023)
        {
          //invalid frequency
          throw new Exception("6, strErrSource, Overflow");
        }
        mFreqDiv = value;
        //if parent is assigned
        if (mSndParent != null)
        {
          //notify parent
          mSndParent.NoteChanged();
        }
      }
    }
    public AGINote()
    {
      strErrSource = "AGINote";
    }
    internal AGINote(AGISound parent, AGITrack tparent)
    {
      mSndParent = parent;
      mTrkParent = tparent;
      strErrSource = "AGINote";
    }
    public AGINote(int freqdiv, int duration, byte attenuation)
    {
      strErrSource = "AGINote";
      //validate freqdiv
      if (freqdiv < 0 || freqdiv > 1023)
      {
        //invalid frequency
        throw new Exception("6, strErrSource, Overflow");
      }
      mFreqDiv = freqdiv;
      //validate duration
      if (duration < 0 || duration > 0xFFFF)
      {
        //invalid frequency
        throw new Exception("6, strErrSource, Overflow");
      }
      mDuration = duration;
      //validate attenuation
      if (attenuation > 15)
      {
        //invalid item
        throw new Exception("6, strErrSource, Overflow");
      }
      mAttenuation = attenuation;
    }
  }
}
