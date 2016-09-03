using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace BowlingScore {
	class Program {
		static int Main( string[] args ) {

			if ( args.Length == 0 ) {
				Console.WriteLine( "Useage : BowlingScore.exe FilePath" );
				Console.ReadKey();
				return 255;
			}

			if ( !File.Exists( args[ 0 ] ) ) {
				Console.WriteLine( "File Not Found!! [" + args[ 0 ] + "] " );
				Console.ReadKey();
				return 255;
			}

			if ( new FileInfo( args[ 0 ] ).Length == 0 ) {
				Console.WriteLine( "Invalid File!! [" + args[ 0 ] + "] " );
				Console.ReadKey();
				return 255;
			}

			PlayData playData = getJSON<PlayData>( args[ 0 ] );

			bool isFrameFinish = false;
			int totalScore = 0, frameScore = 0, frameCount = 1;

			foreach ( var elem in playData.game_data.OrderBy( x => x.getDate() ).Select( ( x, i ) => new { game_data = x, index = i } ) ) {

				int addCount = 0;

				int pin = ( elem.game_data.foul ) ? 0 : elem.game_data.pin;
				frameScore += pin;
				totalScore += pin;

				if ( frameScore >= playData.pin_max ) {
					addCount = ( isFrameFinish ) ? 1 : 2;
					isFrameFinish = true;
				}

				if ( ( addCount > 0 ) && ( frameCount != playData.frame ) ) {
					totalScore += playData.game_data.OrderBy( x => x.getDate() ).Skip( elem.index + 1 ).Take( addCount ).Sum( x => ( x.foul ) ? 0 : x.pin );
				}

				if ( isFrameFinish ) {
					frameScore ^= frameScore;
					isFrameFinish ^= isFrameFinish;

					frameCount = ( frameCount == playData.frame ) ? frameCount : frameCount + 1;

				} else {
					isFrameFinish = true;
				}

			}

			// result
			Console.WriteLine( "Score = " + totalScore );

			// finish
			Console.WriteLine( "finish!" );
			Console.ReadKey();
			return 0;

		}

		private static T getJSON<T>( string filePath ) {

			DataContractJsonSerializer dcjs = new DataContractJsonSerializer( typeof( T ) );
			T ret = default( T );

			using ( MemoryStream ms = new MemoryStream( Encoding.UTF8.GetBytes( File.ReadAllText( filePath, Encoding.UTF8 ) ) ) ) 
			using ( XmlDictionaryReader xdr =	JsonReaderWriterFactory.CreateJsonReader( ms, XmlDictionaryReaderQuotas.Max ) ) {
				ret = (T)dcjs.ReadObject( xdr );
			}

			return ret;
		}

	}

	[DataContract]
	public class PlayData {

		/// <summary>
		/// 全フレーム数
		/// </summary>
		[DataMember( IsRequired = true )]
		public int frame { get; set; }

		/// <summary>
		/// 全ピン数
		/// </summary>
		[DataMember( IsRequired = true )]
		public int pin_max { get; set; }

		/// <summary>
		/// ゲームデータコレクション
		/// </summary>
		[DataMember( IsRequired = true )]
		public GameData[] game_data { get; set; }

		public class GameData {

			/// <summary>
			/// 倒したピン数
			/// </summary>
			[DataMember( IsRequired = true )]
			public int pin { get; set; }

			/// <summary>
			/// スプリット
			/// </summary>
			[DataMember( IsRequired = true )]
			public bool split { get; set; }

			/// <summary>
			/// ファウル
			/// </summary>
			[DataMember( IsRequired = true )]
			public bool foul { get; set; }

			/// <summary>
			/// 投球日時
			/// </summary>
			[DataMember( IsRequired = true )]
			public string date { get; set; }


			// todo : かっこわるいが...
			// DataContractJsonSerializer のフォーマット → "date":"\/Date(946652400000+0900)\/"
			/// <summary>
			/// 投球日時(DateTime)
			/// </summary>
			/// <returns></returns>
			public DateTime getDate() {
				return DateTime.Parse( this.date );
			}
		}
	}

}
