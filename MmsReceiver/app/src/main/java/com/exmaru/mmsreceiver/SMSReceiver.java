package com.exmaru.mmsreceiver;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.telephony.SmsMessage;
import android.text.TextUtils;
import android.util.Log;

import java.util.ArrayList;

public class SMSReceiver extends BroadcastReceiver
{
    @Override
    public void onReceive(Context $context, Intent $intent)
    {
        Bundle bundle = $intent.getExtras();

        if (bundle == null)
            return;

        Object[] pdus = (Object[]) bundle.get("pdus");
        if (pdus == null)
            return;

        ArrayList<String> msgs = new ArrayList<String>();
        String number = "";
        for (int i = 0; i < pdus.length; i++)
        {
            SmsMessage smsMsg = SmsMessage.createFromPdu((byte[]) pdus[i]);
            number = smsMsg.getDisplayOriginatingAddress();

            msgs.add(smsMsg.getDisplayMessageBody());
        }

        if (!TextUtils.isEmpty(number) && number.contains(";"))
            number = number.split(";")[0];

        if (!TextUtils.isEmpty(number))
            number = number.trim().replaceAll("[^0-9]", "");

        String msg = TextUtils.join(" ", msgs);
        Log.i("SMSReceiver.java | onReceive", "|" + number + "|" + msg);
    }
}