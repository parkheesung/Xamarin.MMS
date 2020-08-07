using Android.App;
using Android.Content;
using Android.Telephony;
using Java.IO;
using OctopusV3.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MmsReceiver.Droid
{
    public class Repository
    {
        public static ReturnValues<List<long>> FindLastKey(ContentResolver resolver)
        {
            var result = new ReturnValues<List<long>>();

            try
            {
                System.String[] reqCols = new System.String[] { "_id" };
                Android.Net.Uri sentURI = Android.Net.Uri.Parse($"content://mms/inbox");
                var cursor = resolver.Query(sentURI, reqCols, null, null, "date DESC");

                if (cursor != null)
                {
                    long point = 0;
                    List<long> cursors = new List<long>();

                    for (int i = 0; i < cursor.Count && cursor != null; i++)
                    {
                        if (cursor.MoveToFirst())
                        {
                            cursor.Move(i);
                            string tmp = cursor.GetString(cursor.GetColumnIndex("_id"));
                            if (!string.IsNullOrWhiteSpace(tmp))
                            {
                                if (long.TryParse(tmp, out point))
                                {
                                    cursors.Add(point);
                                }
                            }
                        }
                    }

                    if (cursors != null && cursors.Count > 0)
                    {
                        result.Success(cursors.Max(), cursors);
                    }
                    else
                    {
                        result.Error("Not Found");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error(ex);
            }

            return result;
        }

        public static string GetMyPhoneNumber(Context resolver)
        {
            TelephonyManager mgr = Application.Context.GetSystemService(Context.TelephonyService) as TelephonyManager;
            return mgr.Line1Number;
        }

        public static string getPhone(ContentResolver resolver, long id)
        {
            string name = string.Empty;

            try
            {
                string selectionAdd = $"msg_id={id}";
                Android.Net.Uri uriAddress = Android.Net.Uri.Parse($"content://mms/{id}/addr");
                var cAdd = resolver.Query(uriAddress, null, selectionAdd, null, null);

                if (cAdd.MoveToFirst())
                {
                    name = cAdd.GetString(cAdd.GetColumnIndex("address"));
                }
            }
            catch
            {

            }

            return name;
        }

        public static string getMmsText(ContentResolver resolver, long id)
        {
            Android.Net.Uri partURI = Android.Net.Uri.Parse($"content://mms/part/{id}");
            StringBuilder sb = new StringBuilder(200);
            try
            {
                using (var input = resolver.OpenInputStream(partURI))
                {
                    if (input != null)
                    {
                        InputStreamReader isr = new InputStreamReader(input, "UTF-8");
                        BufferedReader reader = new BufferedReader(isr);
                        string temp = reader.ReadLine();
                        while (temp != null)
                        {
                            sb.Append(temp);
                            temp = reader.ReadLine();
                        }
                    }
                }
            }
            catch
            {
            }

            return sb.ToString();
        }

        public static Coupon GetMMS(ContentResolver resolver, long lastID)
        {
            Coupon coupon = null;

            Android.Net.Uri uri = Android.Net.Uri.Parse($"content://mms/part");
            string selection = $"_id={lastID}";
            var query = resolver.Query(uri, null, selection, null, null);
            if (query.MoveToFirst())
            {
                do
                {
                    try
                    {
                        coupon = new Coupon();
                        coupon.ID = query.GetString(query.GetColumnIndex("_id"));
                        coupon.ReceiveType = query.GetString(query.GetColumnIndex("ct"));
                        coupon.Phone = getPhone(resolver, lastID);

                        string data = query.GetString(query.GetColumnIndex("_data"));
                        if (data != null)
                        {
                            coupon.Body = getMmsText(resolver, lastID);
                        }
                        else
                        {
                            coupon.Body = query.GetString(query.GetColumnIndex("text"));
                        }
                    }
                    catch
                    {
                    }

                } while (query.MoveToNext());
            }

            return coupon;
        }
    }

    public static class ExtendRepository
    {
        public static ReturnValues<List<long>> FindLastKey(this ContentResolver resolver)
        {
            return Repository.FindLastKey(resolver);
        }

        public static string GetMyPhoneNumber(this Context resolver)
        {
            return Repository.GetMyPhoneNumber(resolver);
        }

        public static string getPhone(this ContentResolver resolver, long id)
        {
            return Repository.getPhone(resolver, id);
        }

        public static string getMmsText(this ContentResolver resolver, long id)
        {
            return Repository.getMmsText(resolver, id);
        }

        public static Coupon GetMMS(this ContentResolver resolver, long lastID)
        {
            return Repository.GetMMS(resolver, lastID);
        }
    }
}