using EfiSharp;
using Internal.Runtime.CompilerHelpers;
using Internal.Runtime.NativeFormat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;


namespace EfiSharp
{


    public unsafe class VmbusBufferByteListMapEnumerator : IEnumerator<byte>, System.Collections.IEnumerator
    {
        private List<ByteList> btls;
        private int _Count = 0;
        private int index = 0;
        private byte _Current = 0;
        public VmbusBufferByteListMapEnumerator()
        {
            btls = new List<ByteList>();
        }
        public int Count
        {
            get
            {
                return _Count;
            }
        }

        public byte this[int index]
        {
            get
            {
                return BufferAtIndex(index);

            }

        }

        public void Add(ByteList t)
        {
            btls.Add(t);
            _Count += t.Count;

        }




        public void RemoveRange(int index, int count)
        {

            int nowidx = 0;
            byte ret = 0;
            List<int> removerng = new List<int>();
            int rmidx = 0;
            foreach (ByteList bt in btls)
            {
                int savecount = bt.Count;
                if (index>=nowidx&&index < nowidx + savecount)
                {
                   
                    int starttrim = index - nowidx;
                    int offset = index + count - nowidx;
                    if (offset > savecount)
                    {
                        offset = savecount;
                    }

                    offset -= starttrim;
                    if (offset > 0)
                    {
                        bt.RemoveRange(starttrim, offset);
                        if (index <= nowidx && index + count >= nowidx + savecount)
                        {
                            removerng.Add(rmidx);

                        }

                        if (index + count - nowidx > savecount)
                        {
                            index = nowidx + savecount;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

               
                nowidx += savecount;
                rmidx++;
            }

            foreach (int i in removerng)
            {
                btls[i].Dispose();

                btls.RemoveAt(i);
            }
            removerng.Dispose();

            _Count = (int)btls.Sum(h => h.Count);
            return;
        }

        public void Clear()
        {
            _Count = 0;
            foreach (ByteList bt in btls)
            {
               // bt.Clear();
                bt.Dispose();
            }
            btls.Clear();
        }


        public byte BufferAtIndex(int i)
        {
            int nowidx = 0;
            byte ret = 0;
            foreach (ByteList bt in btls)
            {
                if (i>= nowidx&&i<  nowidx + bt.Count)
                {
                    int offset = i - nowidx;
                    return bt[offset];
                }
                nowidx += bt.Count;
            }

            return ret;
        }



        public bool MoveNext()
        {
            if (index < _Count)
            {
                _Current = BufferAtIndex(index);
                index++;
                return true;
            }
            return MoveNextRare();
        }
        private bool MoveNextRare()
        {

            return false;
        }

        Object System.Collections.IEnumerator.Current
        {
            get
            {
                return _Current;
            }
        }


       public byte Current
        {
            get
            {
                return _Current;
            }
        }
        public void Reset()
        {
            index = 0;
        }

       
    }

    public class VmbusBufferByteListMapEnumerableWrapper : IEnumerable<byte>, System.Collections.IEnumerable
    {
        private VmbusBufferByteListMapEnumerator _emu;
        public VmbusBufferByteListMapEnumerableWrapper(VmbusBufferByteListMapEnumerator emu)
        {
            _emu = emu;
        }

        public int Count
        {
            get
            {
                return _emu.Count;
            }
        }

        public byte BufferAtIndex(int i)
        {
            return _emu.BufferAtIndex(i);
        }

        public void CopyTo(ByteList ls)
        {
            foreach (byte tmp in this)
            {
                ls.Add(tmp);
            }

            return;
        }

        public void Dump()
        {
            ByteList ls=new ByteList();
            CopyTo(ls);
            Console.HexDump(ls);
            ls.Dispose();

        }

        public void CopyTo(ByteList ls, int offset, int count)
        {
            if (offset > this.Count)
            {
                return;
            }
            UInt32 lenend = offset + count;

            if (lenend > this.Count)
            {
                lenend = this.Count;
            }
            for (int i = offset; i < lenend; i++)
            {
                byte tmp = this[i];
                ls.Add(tmp);
            }
            return;
        }

        public void Clear()
        {
            _emu.Clear();
            return;
        }

        public void Add(ByteList t)
        {
            _emu.Add(t);
            return;
        }

        public void RemoveRange(int index, int count)
        {
            _emu.RemoveRange(index, count);
            return;
        }

        public byte this[int index]
        {
            get
            {
                return BufferAtIndex(index);

            }

        }
        public  System.Collections.IEnumerator  GetEnumerator()
        {
            _emu.Reset();
            return _emu;
        }

        IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
        {
            _emu.Reset();
            return _emu as  IEnumerator<byte>;
        }
    }
    public unsafe class VmbusBufferDictionaryEnumerator : IEnumerator<UInt32>, System.Collections.IEnumerator
    {
        private List<ByteList> btls;
        private int index = 0;
        private int _Count = 0;
        private UInt32 _Current = 0;
        private bool copymode = false;
        public VmbusBufferDictionaryEnumerator(List<ByteList> buffermap)
        {
            int alllen = (int)buffermap.Sum(h => h.Count);
            btls = buffermap;
            _Count = alllen;
            copymode = false;
        }
        public VmbusBufferDictionaryEnumerator(ByteList buffermap, int offset, int count)
        {
            btls = new List<ByteList>();
            ByteList btlstmp = new ByteList(count);
            buffermap.CopyTo(btlstmp, offset, count);
            btls.Add(btlstmp);
            _Count = count;
            copymode = true;
        }
        public int Count
        {
            get
            {
                return _Count / 4;
            }
        }

        private byte BufferAtIndex(int i)
        {
            int nowidx = 0;
            byte ret = 0;
            foreach (ByteList bt in btls)
            {
                if (i < nowidx + bt.Count)
                {
                    int offset = i - nowidx;

                    return bt[offset];
                }
                nowidx += bt.Count;
            }

            return ret;
        }
        public bool MoveNext()
        {
            UInt32 checksumtmp = 0;

            if ((index < _Count))
            {
                int remain = _Count - index;
                if (remain > 4)
                {
                    remain = 4;
                }
                for (int i = 0; i < remain; i++)
                {
                    UInt32 tmp = (UInt32)BufferAtIndex(index + i);

                    checksumtmp |= (tmp << (i * 8));
                }



                _Current = checksumtmp;
                index += remain;
                return true;
            }
            return MoveNextRare();
        }
        private bool MoveNextRare()
        {

            return false;
        }

        public override void Dispose()
        {
            if (copymode)
            {
                foreach (ByteList btl in btls)
                {
                    btl.Dispose();
                }

                btls.Dispose();
            }
            base.Dispose();
        }

        object IEnumerator.Current => Current;

        public void Reset()
        {
            index = 0;
        }

        public uint Current
        {
            get
            {
                return _Current;
            }
        }
    }

    public unsafe class Vmbus
    {
        [DllImport("NativeUefi", EntryPoint = "HvHvSignalEvent")]
        public static extern void HvHvSignalEvent(UInt32 sig);
        [DllImport("NativeUefi", EntryPoint = "channel_child_relid_synic_event_page_sint")]
        public static extern bool channel_child_relid_synic_event_page_sint();

        public Vmbus()
        {
        }

        private static unsafe hv_device gpipedev = null;
        private static volatile UInt32 requestid = 1;

        private static bool SyncFeedBack = false;

        private static byte[] ManualDropRingBuffer;

        private static VmbusBufferByteListMapEnumerableWrapper ReceiveQueue;
        /* Get the size of the ring buffer */
        private static unsafe UInt32 hv_get_ring_buffersize(hv_ring_buffer_info ring_info)
        {
            return ring_info.ring_datasize;
        }


        private static unsafe IntPtr hv_get_ring_buffer(hv_ring_buffer_info ring_info)
        {

            return (IntPtr)ring_info.buf;
        }

        private static unsafe UInt32 hv_get_bytes_to_write(hv_ring_buffer_info rbi)
        {
            UInt32 read_loc, write_loc, dsize, write;

            dsize = rbi.ring_datasize;
            hv_ring_buffer* ring_buffer = (hv_ring_buffer*)rbi.ring_buffer;

            read_loc = ring_buffer->read_index;
            write_loc = ring_buffer->write_index;


            write = write_loc >= read_loc ? dsize - (write_loc - read_loc) : read_loc - write_loc;
            return write;

        }



        /* Get the next write location for the specified ring buffer */
        private static unsafe UInt32 hv_get_next_write_location(hv_ring_buffer_info rbi)
        {
            hv_ring_buffer* ring_buffer = (hv_ring_buffer*)(rbi.ring_buffer);

            UInt32 next = ring_buffer->write_index;

            return next;

        }

        private static unsafe UInt32
                hv_get_next_read_location(hv_ring_buffer_info rbi)
        {
            hv_ring_buffer* ring_buffer = (hv_ring_buffer*)rbi.ring_buffer;

            UInt32 next = ring_buffer->read_index;

            return next;


        }


        /* Set the next write location for the specified ring buffer */
        private static unsafe void hv_set_next_write_location(hv_ring_buffer_info rbi,
                UInt32 next_write_location)
        {
            hv_ring_buffer* ring_buffer = (hv_ring_buffer*)rbi.ring_buffer;

            ring_buffer->write_index = next_write_location;
            return;

        }




        private static unsafe UInt64 hv_get_ring_bufferindices(hv_ring_buffer_info rbi)
        {
            hv_ring_buffer* ring_buffer = (hv_ring_buffer*)rbi.ring_buffer;

            return (UInt64)ring_buffer->write_index << 32;

        }



        /* Set the next write location for the specified ring buffer */
        private static unsafe void hv_set_next_read_location(hv_ring_buffer_info rbi,
        UInt32 next_read_location)
        {
            hv_ring_buffer* ring_buffer = (hv_ring_buffer*)rbi.ring_buffer;

            ring_buffer->read_index = next_read_location;
            return;

        }

        private static UInt32 hv_copyto_ringbuffer(hv_ring_buffer_info ring_info, UInt32 start_write_offset, ByteList src, UInt32 srclen)
        {
            IntPtr ring_buffer = hv_get_ring_buffer(ring_info);
            UInt32 ring_buffer_size = hv_get_ring_buffersize(ring_info);


            if (start_write_offset + srclen > ring_buffer_size)
            {
                UInt32 FragSize = ring_buffer_size - start_write_offset;
                src.Move((ring_buffer + start_write_offset), (int)FragSize);
                UInt32 remainlength = start_write_offset + srclen - ring_buffer_size;
                src.Move((ring_buffer), (int)FragSize, (int)remainlength);
            }
            else
            {
                src.Move((ring_buffer + start_write_offset), (int)srclen);
            }
            start_write_offset += srclen;
            if (start_write_offset >= ring_buffer_size)
                start_write_offset -= ring_buffer_size;


            return start_write_offset;
        }


        private static void hv_dump_ringbuffer(hv_ring_buffer_info ring_info, UInt32 start_write_offset, UInt32 end_write_offset)
        {
            IntPtr ring_buffer = hv_get_ring_buffer(ring_info);

            List<byte> bts = new List<byte>();
            UInt32 len = end_write_offset - start_write_offset;
            if (len == 0)
            {
                return;
            }
            Console.WriteLine("hv_dump_ringbuffer len :=>" + len.ToString("x"));

            for (UInt32 i = start_write_offset; i < end_write_offset; i++)
            {
                byte* stream = (byte*)(ring_buffer + i);

                byte tmp = NativePrimitiveDecoder.ReadUInt8(ref stream);
                bts.Add(tmp);
            }

            Console.HexDump(bts);

            return;
        }


        private static UInt32 hv_pkt_iter_avail(hv_ring_buffer_info rbi)
        {
            UInt32 priv_read_loc = rbi.priv_read_index;
            hv_ring_buffer* ring_buffer = (hv_ring_buffer*)rbi.ring_buffer;
            UInt32 write_loc = (ring_buffer->write_index);
            UInt32 ret = 0;
            if (write_loc >= priv_read_loc)
            {
                ret = write_loc - priv_read_loc;
            }
            else
            {
                ret = (rbi.ring_datasize - priv_read_loc) + write_loc;
            }

            if (ret > Utils.VSM_PAGE_SIZE)
            {
                Console.WriteLine("hv_pkt_iter_avail len:=>" + ret.ToString("x"));
                Debug.Halt();
            }
            return ret;
        }


        private static UInt32 hv_copyfrom_ringbuffer_raw(hv_ring_buffer_info ring_info, UInt32 start_read_offset, IntPtr src, UInt32 srclen)
        {
            IntPtr ring_buffer = hv_get_ring_buffer(ring_info);
            UInt32 ring_buffer_size = hv_get_ring_buffersize(ring_info);

            UInt32 Index = (start_read_offset) % ring_buffer_size;
            if (Index + srclen > ring_buffer_size)
            {
                UInt32 FragSize = ring_buffer_size - Index;
                StartupCodeHelpers.MemcpyPtr(src, (ring_buffer + Index), FragSize);
                start_read_offset = srclen - FragSize;
                StartupCodeHelpers.MemcpyPtr(src + FragSize, ring_buffer, start_read_offset);
            }
            else
            {

                start_read_offset = Index + srclen;
                StartupCodeHelpers.MemcpyPtr(src, (ring_buffer + Index), srclen);
            }

            return start_read_offset;

        }
        private static UInt32 hv_copyfrom_ringbuffer(hv_ring_buffer_info ring_info, UInt32 start_read_offset, ByteList buffer, UInt32 srclen)
        {
            IntPtr ring_buffer = hv_get_ring_buffer(ring_info);
            UInt32 ring_buffer_size = hv_get_ring_buffersize(ring_info);

            UInt32 Index = (start_read_offset) % ring_buffer_size;
            if (Index + srclen > ring_buffer_size)
            {
                UInt32 FragSize = ring_buffer_size - Index;
                buffer.From(ring_buffer + Index, (int)FragSize);
                start_read_offset = srclen - FragSize;
                buffer.From(ring_buffer, 0, (int)FragSize, (int)start_read_offset);
            }
            else
            {

                start_read_offset = Index + srclen;
                buffer.From(ring_buffer + Index, (int)srclen);
            }

            return start_read_offset;

        }


        private static vmpacket_descriptor hv_pkt_iter_first(hv_ring_buffer_info rbi)
        {

            vmpacket_descriptor desc = new vmpacket_descriptor();
            UInt32 ring_buffer_size = hv_get_ring_buffersize(rbi);
            UInt32 hdrlen = desc.GetRawDataSize();
            if (hv_pkt_iter_avail(rbi) < hdrlen)
                return null;

            hv_copyfrom_ringbuffer_raw(rbi,
                rbi.priv_read_index,
                desc.GetRawDataPtr(),
                hdrlen);
            //desc = (struct vmpacket_descriptor*)(hv_get_ring_buffer(rbi) + rbi.priv_read_index);
            UInt32 lenchk = rbi.priv_read_index + hdrlen;
            if (lenchk > rbi.ring_datasize)
            {
                Console.WriteLine("exceed vmpacket_descriptor hv_get_ring_buffer partial" + lenchk.ToString("x"));
                if (rbi.priv_read_index >= ring_buffer_size)
                    rbi.priv_read_index -= ring_buffer_size;
                //Debug.Halt();
            }
            return desc;
        }

        private static vmpacket_descriptor hv_pkt_iter_next(hv_ring_buffer_info rbi, vmpacket_descriptor desc)
        {
            UInt32 VMBUS_PKT_TRAILER = IntPtr.Size;
            UInt32 packetlen = desc.len8 << 3;
            UInt32 ring_buffer_size = hv_get_ring_buffersize(rbi);

            /* bump offset to next potential packet */
            rbi.priv_read_index += packetlen + VMBUS_PKT_TRAILER;
            if (rbi.priv_read_index >= ring_buffer_size)
                rbi.priv_read_index -= ring_buffer_size;

            /* more data? */
            return hv_pkt_iter_first(rbi);

        }
        private static unsafe void hv_pkt_iter_close(hv_device pdev, hv_ring_buffer_info rbi, bool signal)
        {

            hv_ring_buffer* ring_buffer = (hv_ring_buffer*)rbi.ring_buffer;
            UInt32 orig_read_index;
            UInt32 read_index;
            UInt32 write_index;
            UInt32 pending_sz;
            UInt32 orig_free_space, free_space;

            /*
             * Make sure all reads are done before updating the read index since
             * the writer may start writing to the read area once the read index
             * is updated.
             */

            orig_read_index = ring_buffer->read_index;
            ring_buffer->read_index = rbi.priv_read_index;

            /*
             * Older versions of Hyper-V (before WS2012 and Win8) do not
             * implement pending_send_sz and simply poll if the host->guest
             * ring buffer is full. No signaling is needed or expected.
             */
            if (!ring_buffer->feature_bits)
                return;

            /*
             * Issue a full memory barrier before making the signaling decision.
             * If the reading of pending_send_sz were to be reordered and happen
             * before we commit the new read_index, a race could occur.  If the
             * host were to set the pending_send_sz after we have sampled
             * pending_send_sz, and the ring buffer blocks before we commit the
             * read index, we could miss signaling the host.  Issue a full
             * memory barrier to address this.
             */


            /*
             * If the pending_send_sz is zero, then the ring buffer is not
             * blocked and there is no need to signal. This is by far the
             * most common case, so exit quickly for best performance.
             */
            pending_sz = (ring_buffer->pending_send_sz);
            if (!pending_sz)
                return;

            /*
             * Since pending_send_sz is non-zero, this ring buffer is probably
             * blocked on the host, though we don't know for sure because the
             * host may check the ring buffer at any time. In any case, see
             * if we're freeing enough space in the ring buffer to warrant
             * signaling the host. To avoid duplicates, signal the host only if
             * transitioning from a "not enough free space" state to a "enough
             * free space" state. For example, it's possible that this function
             * could run and free up enough space to signal the host, and then
             * run again and free up additional space before the host has a
             * chance to clear the pending_send_sz. The 2nd invocation would be
             * a null transition from "enough free space" to "enough free space",
             * which doesn't warrant a signal.
             *
             * To do this, calculate the amount of free space that was available
             * before updating the read_index and the amount of free space
             * available after updating the read_index. Base the calculation
             * on the current write_index, protected by READ_ONCE() because
             * the host could be changing the value. rmb() ensures the
             * value is read after pending_send_sz is read.
             */

            write_index = (ring_buffer->write_index);

            /*
             * If the state was "enough free space" prior to updating
             * the read_index, then there's no need to signal.
             */
            orig_free_space = (write_index >= orig_read_index)
                ? rbi.ring_datasize - (write_index - orig_read_index)
                : orig_read_index - write_index;
            if (orig_free_space > pending_sz)
                return;

            /*
             * If still in a "not enough space" situation after updating the
             * read_index, there's no need to signal. A later invocation of
             * this routine will free up enough space and signal the host.
             */
            read_index = ring_buffer->read_index;
            free_space = (write_index >= read_index)
                ? rbi.ring_datasize - (write_index - read_index)
                : read_index - write_index;
            if (free_space <= pending_sz)
                return;

            //++channel->intr_in_full;
            if (signal)
            {
                HvHvSignalEvent(pdev.sig_event);
            }
            return;
        }

        private static unsafe UInt32 hv_ringbuffer_read(hv_device pdev,
                            ByteList buffer, ref UInt32 buffer_actual_len,
                            ref UInt64 prequestid, bool raw, bool signal)
        {
            UInt32 packetlen = 0;
            UInt32 offset;
            buffer_actual_len = 0;
            prequestid = 0;
            vmpacket_descriptor desc = hv_pkt_iter_first(pdev.recv_buf);
            if (desc == null)
            {
                /*
                 * No error is set when there is even no header, drivers are
                 * supposed to analyze buffer_actual_len.
                 */
                return packetlen;
            }



            offset = raw ? 0 : (desc.offset8 << 3);
            packetlen = ((UInt32)desc.len8 << 3) - offset;
            buffer_actual_len = packetlen;
            prequestid = desc.trans_id;

            /*if ((packetlen > buflen))
                return -1;*/

            UInt32 buflen = buffer.Count;
            if (packetlen > buflen)
            {
                packetlen = buflen;
            }
            //UInt32 next_read_location = (UInt32)((UInt64)desc + offset - hv_get_ring_buffer(inring_info));
            hv_ring_buffer_info inring_info = pdev.recv_buf;
            UInt32 desc_location = inring_info.priv_read_index;
            UInt32 next_read_location = desc_location + offset;

            hv_copyfrom_ringbuffer(inring_info,
                next_read_location,
                buffer,
                packetlen);

            /* since ring is double mapped, only one copy is necessary */
            //hvcopymemory(buffer, (void*)((const char*)desc + offset), packetlen);


            /* Advance ring index to next packet descriptor */
            hv_pkt_iter_next(inring_info, desc);

            /* Notify host of update */
            hv_pkt_iter_close(pdev, inring_info, signal);
            /*if (signal)
            {
                HvHvSignalEvent(pdev->sig_event);
            }*/
            return (int)packetlen;
        }

        private static unsafe bool hv_ringbuffer_peek(hv_device pdev)
        {
            UInt32 hdrlen = sizeof(vmpacket_descriptor) + sizeof(VMBUSRING_HDR);
            hv_ring_buffer_info rbi = pdev.recv_buf;
            if (hv_pkt_iter_avail(rbi) < hdrlen)
            {
                return false;

            }
            else
            {
                return true;
            }


        }
        private static bool vmbus_channel_has_data(hv_device pdev)
        {
            return hv_ringbuffer_peek(pdev) == true;
        }

        public static unsafe UInt32 verify_checksum_split(List<ByteList> buffermap)
        {
            UInt32 checksum = 0;
            VmbusBufferDictionaryEnumerator emu = new VmbusBufferDictionaryEnumerator(buffermap);

            EnumerableWrapper<VmbusBufferDictionaryEnumerator, UInt32> warpper = new EnumerableWrapper<VmbusBufferDictionaryEnumerator, UInt32>(emu);

            foreach (UInt32 checksumtmp in warpper)
            {
                checksum = checksum ^ checksumtmp;
            }

            emu.Dispose();
            return checksum;
        }
        public static unsafe UInt32 verify_checksum(ByteList buffermap, int offset, int count)
        {
            UInt32 checksum = 0;
            VmbusBufferDictionaryEnumerator emu = new VmbusBufferDictionaryEnumerator(buffermap, offset, count);

            EnumerableWrapper<VmbusBufferDictionaryEnumerator, UInt32> warpper = new EnumerableWrapper<VmbusBufferDictionaryEnumerator, UInt32>(emu);

            foreach (UInt32 checksumtmp in warpper)
            {
                checksum = checksum ^ checksumtmp;
            }
            emu.Dispose();
            return checksum;
        }
        public static unsafe UInt32 vmbus_sendpacket_checksum_pack(VMBUSRING_HDR hdrchk, UInt32 requestid64,
            List<ByteList> buffermap)
        {

            UInt32 hdrlen = hdrchk.GetRawDataSize();
            UInt32 hdrlen2buf = 0;

            foreach (ByteList ls in buffermap)
            {

                hdrlen2buf += ls.Count;
            }


            hdrchk.magic = HvDef.magichdr;
            hdrchk.magicend = HvDef.magichdrend;
            hdrchk.checksum = 0;
            hdrchk.flag = 1;
            hdrchk.seqnum = requestid64;
            hdrchk.msgsize = hdrlen2buf;
            List<ByteList> buffermapclac = new List<ByteList>();
            buffermapclac.Add(hdrchk.GetRawDataBytes());

            foreach (ByteList ls in buffermap)
            {
                buffermapclac.Add(ls);
            }



            UInt32 checksum = verify_checksum_split(buffermapclac);
            hdrchk.checksum = checksum;


            return checksum;


        }



        /* Write to the ring buffer */
        private static int hv_ringbuffer_write(hv_device pdev, List<kvec> kv_list, UInt32 kv_count)
        {

            int i = 0;
            UInt32 bytes_avail_towrite;
            UInt32 totalbytes_towrite = 0;

            UInt32 next_write_location;
            UInt32 old_write;
            IntPtr prev_indices = 0;
            int failcount = 0;
            hv_ring_buffer_info outring_info = pdev.send_buf;

            if (pdev.rescind)
                return -1;

            for (i = 0; i < kv_count; i++)
                totalbytes_towrite += kv_list[i].iov_len;

            totalbytes_towrite += IntPtr.Size;
            UInt32 ring_buffer_size = hv_get_ring_buffersize(outring_info);

            if (totalbytes_towrite > ring_buffer_size)
            {
                Console.WriteLine("exceed ring_buffer_size :=>" + totalbytes_towrite.ToString("x"));
                return 0;
            }
        rewrite:
            bytes_avail_towrite = hv_get_bytes_to_write(outring_info);

            if (totalbytes_towrite > bytes_avail_towrite)
            {
                failcount++;
                if (failcount > 3)
                {
                    Console.WriteLine("exceed bytes_avail_towrite " + totalbytes_towrite.ToString("x") + " , " + bytes_avail_towrite.ToString("x"));
                    return 0;
                }

                else
                {
                    System.Threading.Thread.Sleep(10);
                    goto rewrite;
                }
                //return -1;
                //	
            }

            /* Write to the ring buffer */
            next_write_location = hv_get_next_write_location(outring_info);

            old_write = next_write_location;


            for (i = 0; i < kv_count; i++)
            {
                if (kv_list[i].iov_len > 0)
                {
                    next_write_location = hv_copyto_ringbuffer(outring_info,
                        next_write_location,
                        kv_list[i].iov_base,
                        kv_list[i].iov_len);
                }
            }


            /* Set previous packet start */
            prev_indices = hv_get_ring_bufferindices(outring_info);

            ByteList endbuf = prev_indices.ToByteList();
            next_write_location = hv_copyto_ringbuffer(outring_info,
                next_write_location,
                endbuf,
                IntPtr.Size);
            endbuf.Dispose();

            /* Now, update the write location */
            hv_set_next_write_location(outring_info, next_write_location);


            // Debug.Halt();

            HvHvSignalEvent(pdev.sig_event);


            //hv_dump_ringbuffer(outring_info, old_write, next_write_location);

            if (pdev.rescind)
                return -1;

            return 0;
        }


        public static ByteList vmbus_receivepacket_windbg_unpack(hv_device pdev, ref UInt32 buffer_actual_len, ref UInt32 replyreq)
        {

            VMBUSRAW_HDR hdrraw = new VMBUSRAW_HDR();
            VMBUSRING_HDR hdrchk = new VMBUSRING_HDR(true);
            UInt32 hdrlenraw = hdrraw.GetRawDataSize();
            UInt32 hdrlen = hdrchk.GetRawDataSize();
            // UInt32 hdrlen2buf = hdrlenraw + hdrlen + buflen + buflennext;
            UInt32 buffer_actual_lenrecieve = 0;
            buffer_actual_len = 0;
            replyreq = 0;
            UInt32 buflenrecieve = Utils.VSM_PAGE_SIZE_DOUBLE;
            /*if (buflenrecieve > hdrlen2buf)
            {
                buflenrecieve = hdrlen2buf;
            }*/

            ByteList ret = new ByteList();
            ManualDropByteList bufferrecieve = new ManualDropByteList(ManualDropRingBuffer);

            UInt64 requestidstack = 0;
            UInt32 retval = hv_ringbuffer_read(pdev, bufferrecieve, ref buffer_actual_lenrecieve, ref requestidstack, false, true);
            if (retval == 0)
            {
                return ret;
            }

            UInt32 write_index = hv_get_next_write_location(pdev.recv_buf);
            hdrraw.SetRawData(bufferrecieve);
            hdrchk.SetRawData(bufferrecieve, (int)hdrlenraw, (int)hdrlen);
            UInt32 passoffset = hdrlenraw + hdrlen;
            if (!((hdrchk.magic == HvDef.magichdr && hdrchk.magicend == HvDef.magichdrend) || (hdrchk.magic == HvDef.magicreplyhdr && hdrchk.magicend == HvDef.magicreplyhdrend && hdrchk.msgsize == 0 && hdrchk.flag == 2)))
            {
                Console.HexDump(bufferrecieve);
                Console.WriteLine("unpack!magic failed,raw " + hdrlenraw.ToString("x") + "," + hdrlen.ToString("x") + "," + buffer_actual_lenrecieve.ToString("x") + "," + write_index.ToString("x") + "," + pdev.recv_buf.priv_read_index);
                Console.WriteLine(hdrraw.ToString());
                Console.WriteLine(hdrchk.ToString());
                return ret;
            }
            UInt32 oldchecksum = hdrchk.checksum;
            int checksumoffset = (int)(passoffset - 8);
            UInt32 checksum = 0;
            int packetlenincluedhdr = (int)(hdrchk.msgsize + hdrlen);
            //回包类型不需要同步hdr
            if (hdrchk.magic == HvDef.magicreplyhdr && hdrchk.magicend == HvDef.magicreplyhdrend)
            {
                buffer_actual_len = 0;



                bufferrecieve.Write(checksumoffset, 0);

                checksum = verify_checksum(bufferrecieve, (int)hdrlenraw, packetlenincluedhdr);
                if (oldchecksum != checksum)
                {
                    Console.HexDump(bufferrecieve);
                    Console.WriteLine("unpack!checksum failed,replyreq " + buffer_actual_lenrecieve.ToString("x") + "," + write_index.ToString("x") + "," + pdev.recv_buf.priv_read_index.ToString("x"));
                    Console.WriteLine(hdrraw.ToString());
                    Console.WriteLine(hdrchk.ToString());
                    return ret;
                }
                replyreq = hdrchk.seqnum;
                if (hdrchk.seqnum != 0)
                {

                    return ret;

                }
                else
                {
                    Console.WriteLine("unpack!seqnum failed,replyreq" + buffer_actual_lenrecieve.ToString("x") + "," + write_index.ToString("x") + "," + pdev.recv_buf.priv_read_index.ToString("x"));
                    Console.WriteLine(hdrraw.ToString());
                    Console.WriteLine(hdrchk.ToString());
                }
            }
            else
            {
                bufferrecieve.Write(checksumoffset, 0);
            }

            buffer_actual_len = 0;
            if (packetlenincluedhdr != hdrraw.msgsize)
            {
                Console.HexDump(bufferrecieve);
                Console.WriteLine("unpack!msgsize failed,dbg " + buffer_actual_lenrecieve.ToString("x") + "," + write_index.ToString("x") + "," + pdev.recv_buf.priv_read_index.ToString("x"));
                Console.WriteLine(hdrraw.ToString());
                Console.WriteLine(hdrchk.ToString());
                return ret;
            }


            checksum = verify_checksum(bufferrecieve, (int)hdrlenraw, packetlenincluedhdr);
            if (oldchecksum != checksum)
            {
                Console.HexDump(bufferrecieve);
                Console.WriteLine("unpack!checksum failed,dbg " + buffer_actual_lenrecieve.ToString("x") + "," + write_index.ToString("x") + "," + pdev.recv_buf.priv_read_index.ToString("x"));
                Console.WriteLine(hdrraw.ToString());
                Console.WriteLine(hdrchk.ToString());
                return ret;
            }
            //KdpDprintf(L"unpack!success  %08x\r\n", hdrchk.msgsize);
            if (hdrchk.msgsize <= buflenrecieve)
            {
                bufferrecieve.CopyTo(ret, (int)passoffset, (int)hdrchk.msgsize);
                buffer_actual_len = hdrchk.msgsize;
            }
            else
            {
                bufferrecieve.CopyTo(ret, (int)passoffset, (int)buflenrecieve);

                buffer_actual_len = buflenrecieve;



            }
            /*if (SyncFeedBack)
            {
                vmbus_sendpacket_acknowledge(&gpipedev, hdrchk.seqnum, VMBUS_DATA_PACKET_FLAG_COMPLETION_REQUESTED);
                if (feedbackseq == 0)
                {
                    feedbackseq = hdrchk.seqnum;
                }
                else
                {
                    UInt32 feedbackseqchk = feedbackseq + 1;
                    if (feedbackseqchk != hdrchk.seqnum)
                    {
                        Print(L"vmbus_sendpacket_acknowledge!dbg %08x %08x \r\n", feedbackseq, hdrchk.seqnum);
                    }

                    feedbackseq = hdrchk.seqnum;
                }
            }*/
            Console.WriteLine("vmbus_receivepacket_windbg_unpack buffer_actual_len:=>"+ buffer_actual_len.ToString("x"));
            bufferrecieve.Dispose();
            return ret;
        }

        private static UInt16 VMBUS_DATA_PACKET_FLAG_COMPLETION_REQUESTED = 1;


        public static ByteList vmbus_receivepacket_windbg(hv_device pdev, ref UInt32 buffer_actual_len, ref UInt32 replyreq)
        {

            int failcount = 0;
            ByteList ret = new ByteList();
            bool hasdata = false;
            bool ContinueOnStack = true;
            replyreq = 0;
            while (failcount < 10)
            {
                if (channel_child_relid_synic_event_page_sint())
                {
                    if (vmbus_channel_has_data(pdev) == false)
                    {
                        Thread.Sleep(10);
                        failcount++;
                    }
                    else
                    {
                        hasdata = true;
                        break;
                    }
                }
                else
                {
                    if (vmbus_channel_has_data(pdev) == false)
                    {
                        Thread.Sleep(10);
                        failcount++;
                    }
                    else
                    {
                        hasdata = true;
                        break;
                    }
                }
            }
            if (hasdata)
            {
                ret = vmbus_receivepacket_windbg_unpack(pdev, ref buffer_actual_len, ref replyreq);

            }
            else if (ContinueOnStack)
            {
                Console.WriteLine("vmbus_receivepacket_windbg!_bittestandreset64 failed failcount:=>" + failcount);
                //Debug.Halt();
            }
            return ret;
        }


        public static void vmbus_sendpacket(hv_device pdev, List<ByteList> buffermap, UInt32 requestid64,
            UInt16 flags)
        {
            VMBUSRING_HDR hdrchk = new VMBUSRING_HDR(true);
            UInt32 hdrlen = hdrchk.GetRawDataSize();
            vmbus_packet_type type = vmbus_packet_type.VM_PKT_DATA_INBAND;
            vmpacket_descriptor desc = new vmpacket_descriptor();
            VMBUSRAW_HDR hdr = new VMBUSRAW_HDR();
            UInt32 hdrlenraw = hdr.GetRawDataSize();
            UInt32 descsize = desc.GetRawDataSize();




            UInt32 bufferlen = buffermap.Sum(h => h.Count);


            UInt32 packetlen = descsize + bufferlen + hdrlen + hdrlenraw;
            UInt32 packetlen_aligned = (UInt32)Utils.ALIGN_UP_FIX(packetlen, IntPtr.Size);

            List<kvec> bufferlist = new List<kvec>(4 + buffermap.Count);
            IntPtr aligned_data = 0;

            /* Setup the descriptor */
            desc.type = (UInt16)type; /* VmbusPacketTypeDataInBand; */
            desc.flags = flags; /* VMBUS_DATA_PACKET_FLAG_COMPLETION_REQUESTED; */
            /* in 8-bytes granularity */
            desc.offset8 = (UInt16)(((UInt16)descsize) >> 3);
            desc.len8 = (UInt16)(packetlen_aligned >> 3);
            desc.trans_id = requestid64;



            ByteList aligned_data_bts = aligned_data.ToByteList();



            UInt32 endfixlen = (packetlen_aligned - packetlen);

            hdr.flags = 1;
            hdr.msgsize = bufferlen + hdrlen;


            vmbus_sendpacket_checksum_pack(hdrchk, requestid64, buffermap);


            bufferlist.Add(new kvec(desc.GetRawDataBytes(), descsize));


            bufferlist.Add(new kvec(hdr.GetRawDataBytes(), hdrlenraw));

            bufferlist.Add(new kvec(hdrchk.GetRawDataBytes(), hdrlen));


            foreach (ByteList buffer in buffermap)
            {
                bufferlist.Add(new kvec(buffer, buffer.Count));


            }

            bufferlist.Add(new kvec(aligned_data_bts.TakePart((int)endfixlen), endfixlen));

            //   Debug.DebugPoint();
            UInt32 allen = bufferlist.Sum(h => h.iov_len);
            Console.WriteLine("hv_ringbuffer_write allen:=>" + allen.ToString("x"));


            hv_ringbuffer_write(pdev, bufferlist, bufferlist.Count);

           foreach (kvec o in bufferlist)
            {
               
                o.Dispose();
            }
            bufferlist.Dispose();
            return;
        }

        public static ByteList KdpReceivePacketVmbus()
        {
            UInt32 buffer_actual_len = 0;
            UInt32 replyreq = 0;
            ByteList ret = vmbus_receivepacket_windbg(gpipedev, ref buffer_actual_len, ref replyreq);
            Console.WriteLine("vmbus_receivepacket_windbg buffer_actual_len " + buffer_actual_len.ToString("x") + ",replyreq " + replyreq.ToString("x"));
            return ret;
        }

        public static UInt32 KdpCalculateChecksum(PSTRING MessageHeader,
            PSTRING MessageData)
        {

            UInt32 Checksum = 0;
            if (MessageHeader != null)
            {
                foreach (byte bt in MessageHeader.Buffer)
                {
                    Checksum += (UInt32)bt;
                }
            }

            if (MessageData != null)
            {
                foreach (byte bt in MessageData.Buffer)
                {
                    Checksum += (UInt32)bt;
                }

            }

            return Checksum;
        }

        public static void KdSendPacketVmbus(
                 KD_PACKET Packet,
             PSTRING MessageHeader,
             PSTRING MessageData,
             KD_CONTEXT KdContext)
        {
            List<ByteList> buffermap = new List<ByteList>();

            UInt16 packetbytecount = 0;

            if (MessageHeader != null)
            {
                packetbytecount += MessageHeader.Length;
            }

            if (MessageData != null)
            {
                packetbytecount += MessageData.Length;
            }

            Packet.ByteCount = packetbytecount;

            Packet.Checksum = KdpCalculateChecksum(MessageHeader, MessageData);

            buffermap.Add(Packet.GetRawDataBytes());


            if (MessageHeader != null)
            {
                if (MessageData != null)
                {

                    MessageData.Buffer.Add(HvDef.PACKET_TRAILING_BYTE);
                    buffermap.Add(MessageHeader.Buffer);
                    buffermap.Add(MessageData.Buffer);

                }
                else
                {
                    MessageHeader.Buffer.Add(HvDef.PACKET_TRAILING_BYTE);
                    buffermap.Add(MessageHeader.Buffer);
                }
            }



            if (KdContext != null)
            {
            }

            UInt32 allen = buffermap.Sum(h => h.Count);
            Console.WriteLine("vmbus_sendpacket :=>"+allen.ToString("x"));
            vmbus_sendpacket(gpipedev, buffermap, requestid, VMBUS_DATA_PACKET_FLAG_COMPLETION_REQUESTED);
            //包含内容已经释放
            buffermap.Dispose();
            // Packet.Dispose();
            requestid++;
            return;
        }

        public static void KdSendPacketVmbus(
            KD_PACKET_ALL Packet,
            KD_CONTEXT KdContext)
        {
            KdSendPacketVmbus(Packet.Packet, Packet.MessageHeader, Packet.MessageData, KdContext);
            Packet.Dispose();
            return;
        }

        public static KD_PACKET_ALL EatPendingManipulatePacketFromReceiveQueue(UInt32 ExpectPacketType)
        {
            KD_PACKET_ALL PendingPacket = new KD_PACKET_ALL();
        RetryReceivePacket:
            KDP_STATUS KdStatus = KdpReceivePacketLeader(ref PendingPacket.Packet.PacketLeader);
            if (KdStatus != KDP_STATUS.KDP_PACKET_RECEIVED)
            {

                Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);

                goto RetryReceivePacket;
            }

            if (PendingPacket.Packet.PacketLeader != HvDef.PACKET_LEADER &&
                PendingPacket.Packet.PacketLeader != HvDef.CONTROL_PACKET_LEADER)
            {
                Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);
                return null;
            }

            KdStatus = KdpReceiveBuffer16(ref PendingPacket.Packet.PacketType);
            if (KdStatus != KDP_STATUS.KDP_PACKET_RECEIVED)
            {

                Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);

                return null;
            }

            if (PendingPacket.Packet.PacketLeader == HvDef.CONTROL_PACKET_LEADER &&
                PendingPacket.Packet.PacketType == HvDef.PACKET_TYPE_KD_RESEND)
            {
                //KdpSendControlPacket(PACKET_TYPE_KD_RESEND, 0);
                ;// goto RetryReceivePacket;
                //return KDP_PACKET_RESEND;

                return null;
            }
            KdStatus = KdpReceiveBuffer16(ref PendingPacket.Packet.ByteCount);
            if (KdStatus != KDP_STATUS.KDP_PACKET_RECEIVED)
            {
                return null;
            }

            if (PendingPacket.Packet.ByteCount > HvDef.PACKET_MAX_SIZE)
            {
                Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);

                return null;
            }

            KdStatus = KdpReceiveBuffer32(ref PendingPacket.Packet.PacketId);
            if (KdStatus != KDP_STATUS.KDP_PACKET_RECEIVED)
            {

                Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);

                return null;
            }

            KdStatus = KdpReceiveBuffer32(ref PendingPacket.Packet.Checksum);
            if (KdStatus != KDP_STATUS.KDP_PACKET_RECEIVED)
            {

                Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);

                return null;
            }

            if (PendingPacket.Packet.PacketLeader == HvDef.CONTROL_PACKET_LEADER)
            {
                switch (PendingPacket.Packet.PacketType)
                {
                    case HvDef.PACKET_TYPE_KD_ACKNOWLEDGE:
                        /* Are we waiting for an ACK packet? */
                        if (ExpectPacketType == HvDef.PACKET_TYPE_KD_ACKNOWLEDGE &&
                            PendingPacket.Packet.PacketId == (Windbg.CurrentPacketId & ~HvDef.SYNC_PACKET_ID))
                        {

                            /* Remote acknowledges the last packet */
                            //注意这个也不需要
                            Windbg.CurrentPacketId ^= 1;

                            return PendingPacket;
                        }
                        else
                        {
                            break;
                            //return KDP_PACKET_RESEND;
                        }
                        /* That's not what we were waiting for, start over */
                        break;

                    case HvDef.PACKET_TYPE_KD_RESET:

                        /*CurrentPacketId = INITIAL_PACKET_ID;
                        RemotePacketId = INITIAL_PACKET_ID;
                        KdpSendControlPacket(PACKET_TYPE_KD_RESET, 0);*/
                        /*KdpSendControlPacket(PACKET_TYPE_KD_ACKNOWLEDGE, INITIAL_PACKET_ID);
                        KdpSymbolReportSynthetic();*/



                        Windbg.CurrentPacketId = HvDef.INITIAL_PACKET_ID;
                        Windbg.RemotePacketId = HvDef.INITIAL_PACKET_ID;
                        Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESET, 0);
                        goto RetryReceivePacket;
                    //	break;
                    //return KDP_PACKET_RECEIVED;
                    /* Fall through */

                    case HvDef.PACKET_TYPE_KD_RESEND:

                        Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);
                        goto RetryReceivePacket;
                    //KdpSendControlPacket(PACKET_TYPE_KD_ACKNOWLEDGE, INITIAL_PACKET_ID);
                    /* Remote wants us to resend the last packet */
                    //return KDP_PACKET_RESEND;

                    default:
                        {

                            /* We got an invalid packet, ignore it and start over */
                            //return KDP_PACKET_RESEND;
                            //continue;
                            break;
                        }
                }
            }
            else if (PendingPacket.Packet.PacketLeader != HvDef.PACKET_LEADER)
            {
                Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);
                goto RetryReceivePacket;
            }

            if (PendingPacket.Packet.PacketType == HvDef.PACKET_TYPE_KD_ACKNOWLEDGE &&
                PendingPacket.Packet.PacketLeader == HvDef.CONTROL_PACKET_LEADER)
            {
                return PendingPacket;
            }

            if (PendingPacket.Packet.PacketType == HvDef.PACKET_TYPE_KD_ACKNOWLEDGE && PendingPacket.Packet.PacketLeader == HvDef.CONTROL_PACKET_LEADER)
            {
                /* We received something different */
                //KdpSendControlPacket(PACKET_TYPE_KD_RESEND, 0);
                /**/

                if (PendingPacket.Packet.PacketId == (Windbg.CurrentPacketId & ~HvDef.SYNC_PACKET_ID))
                {
                    //!确认这个是不是要处理
                    Windbg.CurrentPacketId ^= 1;
                }

                //CurrentPacketId ^= 1;
                return PendingPacket;
            }

            if (PendingPacket.MessageHeader == null)
            {
                PendingPacket.MessageHeader = new PSTRING(new ByteList(HvDef.PACKET_HEADER_SIZE),true);
            }
            KdStatus = KdpReceiveBuffer(PendingPacket.MessageHeader.Buffer,
                PendingPacket.MessageHeader.Length);
            if (KdStatus != KDP_STATUS.KDP_PACKET_RECEIVED)
            {

                Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);

                return null;
            }

            fixed (byte* pbytes = PendingPacket.MessageHeader.Buffer.GetArrayPtr())
            {
                DBGKD_MANIPULATE_STATE64* pManipulateState = (DBGKD_MANIPULATE_STATE64*)pbytes;
                if (!(pManipulateState->ApiNumber >= HvDef.DbgKdApiMin && pManipulateState->ApiNumber <= HvDef.DbgKdApiMax))
                {
                    Console.HexDump(PendingPacket.MessageHeader.Buffer);
                    Debug.Halt();
                    Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);
                    goto RetryReceivePacket;
                }
            }

            UInt32 Checksum = KdpCalculateChecksum(PendingPacket.MessageHeader,
                null);
            
            UInt32 DataLength = PendingPacket.Packet.ByteCount - PendingPacket.MessageHeader.Length;
            byte termByte = 0;
            if (DataLength == 0)
            {
                KdStatus = KdpReceiveBufferByte(ref termByte);

                if (termByte == HvDef.PACKET_TRAILING_BYTE)
                {
                    Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE, PendingPacket.Packet.PacketId);
                    return PendingPacket;
                }
                else
                {
                    Console.WriteLine("PACKET_TRAILING_BYTE unmatch"+ termByte.ToString("x"));
                    return PendingPacket;
                }
            }

            if (PendingPacket.MessageData == null)
            {
                PendingPacket.MessageData = new PSTRING(new ByteList(HvDef.PACKET_MAX_SIZE),true);
                PendingPacket.MessageData.MaximumLength = (UInt16)HvDef.PACKET_MAX_SIZE;
            }

            if (DataLength >= HvDef.PACKET_MAX_SIZE)
            {
                Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);

                goto RetryReceivePacket;
            }

            PendingPacket.MessageData.Length = (UInt16)DataLength;

            KdStatus = KdpReceiveBuffer(PendingPacket.MessageData.Buffer,
                PendingPacket.MessageData.Length);
            if (KdStatus != KDP_STATUS.KDP_PACKET_RECEIVED)
            {

                Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_RESEND, 0);

                return null;
            }

            Checksum += KdpCalculateChecksum(PendingPacket.MessageData,
                null);

            KdStatus = KdpReceiveBufferByte(ref termByte);

            Windbg.KdpSendControlPacket(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE, PendingPacket.Packet.PacketId);

            if (PendingPacket.Packet.Checksum != Checksum)
            {
                Console.WriteLine("PendingPacket.Packet.Checksum " + PendingPacket.Packet.Checksum.ToString("x") + " Checksum " + Checksum.ToString("x"));
            }

            return PendingPacket;
        }

        public static KD_PACKET_ALL EatPendingManipulatePacketPromise(UInt32 ExpectPacketType)
        {
            if (ReceiveQueue.Count == 0)
            {
                CheckReceiveQueueLengthRemain(0x10);
            }

            while (true)
            {
                KD_PACKET_ALL ret = EatPendingManipulatePacketFromReceiveQueue(ExpectPacketType);
                if (ret != null)
                {
                    if (ret.Packet.PacketType == ExpectPacketType)
                    {
                        int allen = 0x10;
                        if (ret.Packet.ByteCount > 0)
                        {
                            allen= ret.Packet.ByteCount + 0x11;
                        }
                       
                        Console.WriteLine("vmbus_receivepacket :=>" + allen.ToString("x"));

                        return ret;
                    }
                }
            }
           
            return null;
        }

        private static bool CheckReceiveQueueLengthRemain(UInt32 LengthRemain)
        {
            if (ReceiveQueue.Count >= LengthRemain)
            {
                return true;
            }
            int failcount = 0;
            bool ContinueOnStack = true;
            while (true)
            {
                UInt32 buffer_actual_len = 0;
                UInt32 replyreq = 0;
                ByteList ReceiveBuffer = vmbus_receivepacket_windbg(gpipedev, ref buffer_actual_len, ref replyreq);
                //
                if (ReceiveBuffer != null)
                {
                    if (ReceiveBuffer.Count > 0)
                    {
                        ReceiveQueue.Add(ReceiveBuffer);
                    }
                }
                if (ReceiveQueue.Count >= LengthRemain)
                {
                    return true;
                }
                else
                {
                    if (failcount > 3 && ContinueOnStack)
                    {
                        Console.WriteLine("CheckReceiveQueueLengthRemain!failed buffer_actual_len:=>" + buffer_actual_len.ToString("x") + ",replyreq:=>" + replyreq.ToString("x")+ ",failcount"+ failcount.ToString()); 
                        Debug.Halt();
                    }

                    failcount++;


                    continue;
                }
            }

            return false;
        }
        public static KDP_STATUS KdpReceiveBuffer(ByteList Buffer, int count)
        {
            CheckReceiveQueueLengthRemain(count);
            UInt32 oldcount = ReceiveQueue.Count;
         
            if (ReceiveQueue.Count >= count)
            {
                ReceiveQueue.CopyTo(Buffer);
                ReceiveQueue.RemoveRange(0, count);
                UInt32 newcount = ReceiveQueue.Count;
                if (oldcount - count != newcount)
                {
                    Console.WriteLine("ReceiveQueue.newcount " + newcount.ToString("x") + ",ReceiveQueue.oldcount " + oldcount.ToString("x") + "," +
                                      count.ToString("x"));
                }

                return KDP_STATUS.KDP_PACKET_RECEIVED;
            }
            else
            {
                return KDP_STATUS.KDP_PACKET_TIMEOUT;
            }
        }
        public static KDP_STATUS KdpReceiveBuffer32(ref UInt32 Buffer)
        {
            int len32 = 4;
            CheckReceiveQueueLengthRemain(len32);
            if (ReceiveQueue.Count >= len32)
            {
                Buffer = UInt32.FromByteList(ReceiveQueue);
                ReceiveQueue.RemoveRange(0, len32);
                return KDP_STATUS.KDP_PACKET_RECEIVED;
            }
            else
            {
                return KDP_STATUS.KDP_PACKET_TIMEOUT;
            }
        }
        public static KDP_STATUS KdpReceiveBufferByte(ref byte Buffer)
        {
            int len1 = 1;
            CheckReceiveQueueLengthRemain(len1);
            if (ReceiveQueue.Count >= len1)
            {
                Buffer = ReceiveQueue[0];
                ReceiveQueue.RemoveRange(0, len1);
                return KDP_STATUS.KDP_PACKET_RECEIVED;
            }
            else
            {
                return KDP_STATUS.KDP_PACKET_TIMEOUT;
            }
        }
        public static KDP_STATUS KdpReceiveBuffer16(ref UInt16 Buffer)
        {
            int len16 = 2;
            CheckReceiveQueueLengthRemain(len16);
            if (ReceiveQueue.Count >= len16)
            {
                Buffer = UInt16.FromByteList(ReceiveQueue);
                ReceiveQueue.RemoveRange(0, len16);
                return KDP_STATUS.KDP_PACKET_RECEIVED;
            }
            else
            {
                return KDP_STATUS.KDP_PACKET_TIMEOUT;
            }
        }
        public static KDP_STATUS KdpReceivePacketLeader(ref UInt32 PacketLeader)
        {
            retry:
            CheckReceiveQueueLengthRemain(4);
            ByteList Buffer = new ByteList(4);


            int startidx = 0;

            int trimidx = 0;
            byte tmpold = 0;

            for (int i = 0; i < ReceiveQueue.Count; i++)
            {
                byte tmp = ReceiveQueue[i];
                if (tmp == HvDef.PACKET_LEADER_BYTE ||
                    tmp == HvDef.CONTROL_PACKET_LEADER_BYTE)
                {
                    Buffer.Add(tmp);
                    if (Buffer.Count > 0)
                    {
                        if (tmp != Buffer[0])
                        {
                            Buffer.Clear();
                            continue;
                        }
                    }

                    if (Buffer.Count == 4)
                    {
                       
                        if (Buffer.AllSame())
                        {
                           
                            PacketLeader = UInt32.FromByteList(Buffer);
                            Console.WriteLine("FromByteList PacketLeader:=>"+ PacketLeader.ToString("x"));
                            trimidx = i + 1;
                            break;
                        }
                        else
                        {
                            Buffer.Clear();
                        }
                    }
                }

            }

            if (trimidx != 0)
            {
                ReceiveQueue.RemoveRange(0, trimidx);

                return KDP_STATUS.KDP_PACKET_RECEIVED;
            }
            else
            {
                ReceiveQueue.Clear();
                goto retry;
                return KDP_STATUS.KDP_PACKET_TIMEOUT;
            }

        }
        public static KD_PACKET_ALL KdReceivePacket(UInt32 PacketType, KD_CONTEXT KdContext)
        {


            return null;
        }

        [RuntimeExport("SyncRingBufferRoot")]
        public static unsafe void SyncRingBufferRoot(UInt64 bufpage, UInt32 sig_event)
        {
            bool dumpstack = false;
            UInt32 buflenrecieve = Utils.VSM_PAGE_SIZE_DOUBLE;

            ManualDropRingBuffer = new byte[buflenrecieve];

           
            VmbusBufferByteListMapEnumerator ReceiveQueueObj = new VmbusBufferByteListMapEnumerator();

            ReceiveQueue = new VmbusBufferByteListMapEnumerableWrapper(ReceiveQueueObj);


            gpipedev = new hv_device();
            const UInt32 pagecount = 6;
            const UInt32 pagecountsplit = 3;
            const UInt32 allpagesize = pagecount * Utils.VSM_PAGE_SIZE;
            const UInt32 splitpagesize = Utils.VSM_PAGE_SIZE * pagecountsplit;
            gpipedev.sig_event = sig_event;
            //  gpipedev.send_buf.bufpage = bufpage;
            UInt64 buffirstpage = bufpage;
            gpipedev.send_buf.ring_buffer = buffirstpage;
            gpipedev.send_buf.buf = gpipedev.send_buf.ring_buffer + Utils.VSM_PAGE_SIZE;
            gpipedev.send_buf.buf_size = splitpagesize;
            gpipedev.send_buf.priv_read_index = 0;
            gpipedev.send_buf.ring_datasize = Utils.VSM_PAGE_SIZE_DOUBLE;
            //  gpipedev.recv_buf.bufpage = bufpage + (pagecountsplit * Utils.VSM_PAGE_SIZE);
            UInt64 bufsecondpage = bufpage + splitpagesize;
            if (dumpstack)
            {
                Console.WriteLine(bufpage.ToString("x") + "," + bufsecondpage.ToString("x"));
            }

            gpipedev.recv_buf.ring_buffer = bufsecondpage;
            gpipedev.recv_buf.buf = bufsecondpage + Utils.VSM_PAGE_SIZE;
            gpipedev.recv_buf.buf_size = splitpagesize;
            gpipedev.recv_buf.ring_datasize = Utils.VSM_PAGE_SIZE_DOUBLE;
            gpipedev.recv_buf.priv_read_index = 0;
            if (dumpstack)
            {
                IntPtr send_bufptr = gpipedev.send_buf;
                IntPtr recv_bufptr = gpipedev.recv_buf;
                Console.WriteLine(send_bufptr.ToString("x") + "," + recv_bufptr.ToString("x"));
                Console.WriteLine(gpipedev.ToString());
                Debug.Halt();
            }

            return;
        }

    }
}
