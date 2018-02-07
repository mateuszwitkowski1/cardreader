using System;
using System.Text;

using PCSC; 

namespace HelloWorld
{
	class Program
	{
		static void CheckErr(SCardError err)
		{
			if (err != SCardError.Success)
				throw new PCSCException(err,
					SCardHelper.StringifyError(err));
		}
		static void Main(string[] args)
		{
	

			try
			{
				string imie;
				string nazwisko;
				string pesel;
				string nr_indeksu;
				// Establish SCard context
				SCardContext hContext = new SCardContext();
				hContext.Establish(SCardScope.System);

				// Retrieve the list of Smartcard readers
				string[] szReaders = hContext.GetReaders();
				if (szReaders.Length <= 0)
					throw new PCSCException(SCardError.NoReadersAvailable,
						"Could not find any Smartcard reader.");

				Console.WriteLine("reader name: " + szReaders[0]);

				// Create a reader object using the existing context
				SCardReader reader = new SCardReader(hContext);

				// Connect to the card
				SCardError err = reader.Connect(szReaders[0],
					SCardShareMode.Shared,
					SCardProtocol.T0 | SCardProtocol.T1);
				CheckErr(err);

				System.IntPtr pioSendPci;
				switch (reader.ActiveProtocol)
				{
				case SCardProtocol.T0:
					pioSendPci = SCardPCI.T0;
					break;
				case SCardProtocol.T1:
					pioSendPci = SCardPCI.T1;
					break;
				default:
					throw new PCSCException(SCardError.ProtocolMismatch,
						"Protocol not supported: "
						+ reader.ActiveProtocol.ToString());
				}

				byte[] SELECT_MF = new byte[] { 0x00,  0xA4,  0x00,  0x0C,  0x02,  0x3F,  0x00};
				byte[] SELECT_DF_T0 = new byte[] { 0x00,  0xA4,  0x04,  0x00,  0x07,  0xD6,  0x16,  0x00,  0x00,  0x30,  0x01,  0x01};
				byte[] SELECT_DF_T1 = new byte[] { 0x00,  0xA4,  0x04,  0x04,  0x07,  0xD6,  0x16,  0x00,  0x00,  0x30,  0x01,  0x01};
				byte[] SELECT_ELS_T0 = new byte[] { 0x00,  0xA4,  0x00,  0x00,  0x02,  0x00,  0x02};
				byte[] SELECT_ELS_T1 = new byte[] { 0x00,  0xA4,  0x02,  0x04,  0x02,  0x00,  0x02};
				byte[] READ_ELS = new byte[] { 0x00,  0xB0,  0x00,  0x00,  0xFF};

				//--
				byte[] pbRecvBuffer = new byte[256];

				// Send SELECT command
				err = reader.Transmit(pioSendPci, SELECT_MF, ref pbRecvBuffer);
				CheckErr(err);

				Console.Write("select_mf: ");
				for (int i = 0; i < pbRecvBuffer.Length; i++)
					Console.Write("{0:X2} ", pbRecvBuffer[i]);
				Console.WriteLine();
				//--

				//--
				pbRecvBuffer = new byte[256];

				// Send SELECT command
				err = reader.Transmit(pioSendPci, SELECT_DF_T0, ref pbRecvBuffer);
				CheckErr(err);

				Console.Write("select_df_t0: ");
				for (int i = 0; i < pbRecvBuffer.Length; i++)
					Console.Write("{0:X2} ", pbRecvBuffer[i]);
				Console.WriteLine();
				//--

				//--
				pbRecvBuffer = new byte[256];

				//--
				pbRecvBuffer = new byte[256];

				// Send SELECT command
				err = reader.Transmit(pioSendPci, SELECT_ELS_T0, ref pbRecvBuffer);
				CheckErr(err);

				Console.Write("select_els_t0: ");
				for (int i = 0; i < pbRecvBuffer.Length; i++)
					Console.Write("{0:X2} ", pbRecvBuffer[i]);
				Console.WriteLine();
				//--

				//--
				pbRecvBuffer = new byte[2048];

				// Send SELECT command
				err = reader.Transmit(pioSendPci, READ_ELS, ref pbRecvBuffer);
				CheckErr(err);
				Console.Write("read_els: ");
				nr_indeksu="";
				pesel="";
				for (int i = 0; i < pbRecvBuffer.Length; i++){
					//index
					if(pbRecvBuffer[i]==0x06 && i<149 && i>120){
						int a=i;
						nr_indeksu += Convert.ToChar(pbRecvBuffer[a+1]);
						nr_indeksu += Convert.ToChar(pbRecvBuffer[a+2]);
						nr_indeksu += Convert.ToChar(pbRecvBuffer[a+3]);
						nr_indeksu += Convert.ToChar(pbRecvBuffer[a+4]);
						nr_indeksu += Convert.ToChar(pbRecvBuffer[a+5]);
						nr_indeksu += Convert.ToChar(pbRecvBuffer[a+6]);
					}
					//koniec index

					//nazwisko

					//koniec nazwisko

					//imie

					//koniec imie

					//pesel

					if(pbRecvBuffer[i]==0x0B && i>135 && i<155){
						int a=i;
						pesel += Convert.ToChar(pbRecvBuffer[a+1]);
						pesel += Convert.ToChar(pbRecvBuffer[a+2]);
						pesel += Convert.ToChar(pbRecvBuffer[a+3]);
						pesel += Convert.ToChar(pbRecvBuffer[a+4]);
						pesel += Convert.ToChar(pbRecvBuffer[a+5]);
						pesel += Convert.ToChar(pbRecvBuffer[a+6]);
						pesel += Convert.ToChar(pbRecvBuffer[a+7]);
						pesel += Convert.ToChar(pbRecvBuffer[a+8]);
						pesel += Convert.ToChar(pbRecvBuffer[a+9]);
						pesel += Convert.ToChar(pbRecvBuffer[a+10]);
						pesel += Convert.ToChar(pbRecvBuffer[a+11]);
					}
					//koniec pesel
					Console.Write(i + "\t");
					Console.Write("{0:X2} ", pbRecvBuffer[i]);
					Console.Write("\t" + Convert.ToChar(pbRecvBuffer[i]));
					Console.WriteLine();

				}
				Console.WriteLine();

				//--

				hContext.Release();
				foreach(byte a in pbRecvBuffer){
					Console.Write("{0:X2}", a);
				}
				//var str = System.Text.Encoding.Default.GetString(pbRecvBuffer);
				Console.WriteLine("Nr indeksu: " + nr_indeksu);
				Console.WriteLine("Pesel: " + pesel);
				Console.ReadKey();

			}
			catch (PCSCException ex)
			{
				Console.WriteLine("Ouch: "
					+ ex.Message
					+ " (" + ex.SCardError.ToString() + ")");
			}
		}
	}
}
