# Battle botları ve mod sınırı

2026-10-08 — Mod/Bot uygulaması. PC kabul sonucu aşağıdaki kanıt kaydında güncellenir; Android performansı ve uzun oturum dengesi bu aşamada ölçülmüş sayılmaz.

## Oyuncunun göreceği

Varsayılan beş vagonlu Battle: bir insan ve dört yerel bot (Ray, Mira, Kaya, Nova). Üç profil Rookie/Regular/Veteran; ikinci Rookie aynı kurallarla ayrı bir katılımcıdır. Yakındaki botlar gerçek kapsül, hareket, atış izi ve ad etiketiyle görülür. `[BOT]` etiketi yerel bilgisayar rakibini belirtir; gerçek ağ eşleşmesi kurulmadı. Geçici HUD'daki `Switch to SOLO/BATTLE` yeni koşu başlatır; botsuz mod yalnız ortak ilerleme döngüsüdür, hikâye içeriği henüz yoktur.

Kullanıcının seçtiği bitiş: insan ölünce mevcut sırası ile koşu biter; tek yaşayan insan kalınca birincilikle biter. Bir karede elenenler aynı sırayı paylaşır (5 kişi, 2 eşzamanlı ölüm → ikisi de 4.). Yaşayan diğer rakiplere tamamlanmamış yarışta uydurma son sıra verilmez. Son iki katılımcı aynı karede ölürse ortak birincilik ve beraberlik; yaşayan kazanan yoktur. Sonuç ekranı şimdilik test göstergesidir, kalıcı ödül/reklam teslimi üretmez.

## Sahiplik ve ortak kurallar

- `RunFlow` istasyon evrelerinin tek sahibidir. `RunMatch` mod/eleme/toplu atama politikasını üstlenir; bu politika kapı, zombi veya silah sınıfına gömülmez.
- `BattleRoster` saf sıra/eleme ve eşsiz atama kuralıdır. İnsan için seçilen vagon ayrılır; kalan vagonlar karıştırılıp yalnız yaşayan botlara atanır. Boş/elenmiş vagonlar sonraki atamada tekrar seçilebilir. Her atama bütün katılımcıların fiziksel güvenli noktalarını önce doğrular; biri için yer yoksa kısmi dağıtım yapılmaz, koşu Faulted olur.
- `WagonRuntime.Defender` hem yeni üretimin hem yaşayan zombi hedefinin kaynağıdır. `SetDefender` aktif `EnemyBrain` hedeflerini de günceller. Artık zombiler uzaktaki insanın canına bağlı değildir. Savunmacısı olmayan vagonun mevcut zombileri bekler; yeni zombi programı D27 ile kapanır. İçeride kalanlar kalkışta korunur. Ham havuzun doğrudan üretim API'si laboratuvar kurulumlarına açıktır; normal üretimi `StationSpawner` denetler.
- Her bot ayrı `HealthComponent`, `HitscanWeapon`, `Wallet`, `KillRewards`, `ShopService` kullanır. Takım Player; botlar insana/birbirine ateş etmez. Kendi öldürmesi/kurduğu turretin öldürmesi yalnız kendi cüzdanına gider. Ölü bot ödül/satın alma kabul etmez.
- `PlayerMotor` insan için ekran girdisi, bot için dünya yönü alır; aynı CharacterController ve MovementArea vagon sınırını uygular. Bot NavMesh yolu sorgular, CharacterController yürür; ikinci NavMeshAgent eklenmez. Bir botun sorgu köşeleri 32 ile sınırlıdır; eksik/taşan/dışarı çıkan rota yürütülmez.
- `AutoAim` ortak görüş/atış kullanır; profilde hedef yenileme/tepki gecikmesi vardır. Cam ve kapı üstünden mermi geçer; metal dikme/duvar/alt panel durdurur. Oyuncunun varsayılan .15 sn seçimi ve sıfır gecikmesi korunur.
- `ProximityRepair` aynıdır: 3 saniye, ayrılınca iptal, oyuncu hasarı kesmez/kapı hasarı keser. Bot tehlike yakındaysa kaçar, hasarlı kapıyı seçer ve yerinde onarır; otomatik tamir veya gizli can bonusu yoktur.
- Bot mağazası aynı fiyat/katalog ve senkron işlem çekirdeğini kullanır. Öncelik düşük can → tercih edilen silah/mermi → profil izin verirse normal turret. Mermi yoksa tabancaya döner. Tüm vagon onarımı API'si vardır; mevcut karar politikası bunu otomatik satın almaz. Bot dronu bu ilk sürümün otomatik alışveriş kapsamına eklenmedi.
- Can/para/mermi/silah botla gider; kapı/turret vagonunda kalır. D28 güvenli atama ve aynı ayarlanabilir hasar koruması botlara da uygulanır. Elenen bot aynı koşuda yeniden doğmaz. Restart aynı dört aktörü yeni yaşam kuşağı ve temiz durumla kullanır.
- Karanlık/terminal evrede bot hareket/ateş/onarım/mağaza durur. Ölüm kareleri `LateUpdate` içinde topluca işlenir; çifte bildirim sıra veya ödül yaratmaz.

## Profil ayarları

`Assets/Game/Content/Bots/*.asset` Inspector üzerinden düzenlenir; canlı can/mermi/para bu assetlere yazılmaz.

| Ayar | Rookie | Regular | Veteran |
| --- | ---: | ---: | ---: |
| Karar aralığı, sn | .60 | .35 | .20 |
| Hedef seçimi, sn | .35 | .20 | .12 |
| Hedef değişiminde tepki, sn | .55 | .25 | .08 |
| Yürüme, m/sn | 2.8 | 3.2 | 3.5 |
| Yakın tehlike, m | 1.15 | 1.45 | 1.80 |
| Onarım eşiği, kapı can oranı | .45 | .65 | .85 |
| Can satın alma eşiği | .35 | .50 | .60 |
| Alışveriş karar aralığı, sn | 3 | 2 | 1.3 |
| Tercih silahı | SMG | Rifle | Rifle |
| Turret alımı | Hayır | Hayır | Evet |

Bunlar başlangıç davranış değerleridir, zorluk dengesi kanıtı değildir. İnsanla aynı maksimum can ve **geçici 1000 başlangıç parası** kullanılır; bu nedenle ilk istasyonlar ve alışveriş cömerttir. Daha yüksek beceri, fazladan mermi/para/hasar vermez. Model sözleşmesi: aynı 1.7 × .6 m kapsül, kök ölçek 1, yerel +Z yüz yönü, yalnız collider taşımayan çocuk görseller değişir; [MODEL_CONTRACT.md](MODEL_CONTRACT.md).

## Kurulum ve doğrulama

Mevcut TrainIntegration'da `Wait Your Turn/Battle/Install Bots In Integration` (Ctrl+Alt+F2) yalnız eksik katılımcıları ekler; geometri veya NavMesh'i yeniden üretmez. Tam Integration builder da aynı kurulumu çağırır. Eski tek oyunculu laboratuvarlar RunMatch olmadığı için kendi akışında kalır.

EditMode: `Wait Your Turn/Battle/Run Battle Rules Tests` (Ctrl+Alt+F7), Run test assembly'si. Play: F7 bot kabulü. F8/F9/F10/F11/F12 önceki laboratuvar kontrollerinde Match açıkça bastırılır; sonunda normal ayar ve yeni koşu geri gelir. Bu test izolasyonu üretim modu seçimi değildir.

F7 hedefleri: eşsiz beş katılımcı/üç profil; camdan gerçek öldürme ve ayrı para; metal engeli; gerçek düşmanın botu takip/vurması; ortak onarım hasar ayrımı; gerçek AI hareket/onarım/satın alma/otomatik atış; ölüm/boş vagon üretimi; on geçişte can/mermi/para/kimlik; karanlık duruş; birincilik/insan ölümü; Solo ve tekrar kullanım.

2026-10-08 PC kabulü: 33 Run kural testi ve F7 bot kabulü PASS. Önceki F8 dron, F9 mağaza, F10 tren/cam/metal/kapı, F11 üretim ve F12 turret kontrolleri de PASS; Play kapanışında Console 0 hata/0 uyarı. Sahne incelemesinde eski 6333 serileştirilmiş bloktan hiçbiri kaldırılmadı; geometri ve NavMesh korunur. Kısa ham sonuçlar [generated/bots-pc-acceptance.txt](generated/bots-pc-acceptance.txt). Bu kayıt PC işlev kontrolüdür; Android performansı veya yarış dengesi kanıtı değildir.

## Açık işler

2026-10-09 M8a: katılımcı yaşamı, sıra, vagon, cüzdan/mermi ve insan/bot atama RNG durumu yerel kayda eklendi. Elenmiş bot yüklemede geri doğmaz. D34 ile hedef yokken insan/bot hareket yönüne bakar. Güncel kayıt kabulü ve sınırlar [PERSISTENCE.md](PERSISTENCE.md); aşağıdaki M8 kayıt hedefi bu temel için artık uygulanmıştır.

Beş vagonun bot/zombi mekanikleri şu anda gerçek ve tam simüle edilir; uzaktakiler için yaklaşık savaş hesabı veya sunum seyreltmesi uygulanmadı. Kullanıcının 10 vagonlu yarış için maliyet hedefi korunur. Sonraki optimizasyon, görünürlükten bağımsız durum sahipliği üzerinden ve ölçümle yapılmalıdır; görünmeyen botu kapatıp hayatta saymak veya zombi/can/ödülü sıfırlamak uygun değildir. Android ölçümü, uzun yarışta botların hayatta kalma dağılımı, ekonomi/onarım dengesi ve nihai mobil arayüz ayrıca açık. M8 kayıt; katılımcı yaşamları, eleme sıraları, atanmış vagonlar, cüzdanlar/mermiler ve RNG konumu birlikte kaydedilmelidir.
