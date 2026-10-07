# M3 — tek vagon oynanış parçası

Sahne: `Assets/Game/Scenes/GameplaySandbox.unity`. Açılış: **Wait Your Turn → Gameplay → Open One Wagon**, ardından Play. Kaydedilmiş NavigationSandbox'tan ayrı bir sahne olarak oluşturuldu; aynı `ConnectedNavMesh` ve M2 can/hasar çekirdeğini kullanır. Eski prototip ve önceki laboratuvarlar korunur. Bu sahne istasyon/etap döngüsü değildir; o M4'tedir.

## Kullanıcı tercihleri ve oynama

- Masaüstünde WASD/ok tuşları; mobilde ekranın sol tarafına dokunup sürükleyerek hareket. Girdi adaptörü hareket motorundan ayrıdır.
- En yakın **görüşü açık** zombi otomatik hedeflenir ve vurulur. Sağ joystick gerekmez. Sağlam/onarılmış kapının üst camından dışarıya ateş edilir; alt dolu panel ve duvar görüşü/isabeti keser. Oyuncu kendi kapısına hasar vermez (D15).
- Oyuncu kapı sağlam/kırık/onarılmış olsa da vagon içinde yürür (D16). Bu sınır zombiler veya mermiler için ek collider değildir. İleride görev kodu `PlayerMotor.SetMovementArea(null)` ile izni açabilir; yeni vagonda o vagonun alanı atanır.
- İlk deneme tabancası: 2 hasar, 0.35 s atış aralığı, 10 m menzil, 8 mermi; şarjör boşalınca 1.2 s otomatik doldurma, sınırsız yedek mermi. Bunlar Inspector ayarlarıdır, nihai denge değildir.
- Kırık kapının iç tarafındaki sarı işarete yakın durmak 3 saniyelik onarımı başlatır. Uzaklaşma/ölüm ilerlemeyi sıfırlar; yarım onarım can vermez. HUD yalnızca ilerlemeyi okur.
- Başlangıçta 3 zombi vardır. `+1 / +6` deneme üretimi ve `Restart` yeni test koşusu içindir. İstasyon spawn programı veya mağaza değildir.

## Sistemdeki sorumluluklar

| Parça | Sorumluluk / bağımlılık |
| --- | --- |
| `DoorController` (Train) | Ortak dayanıklılık → kırılma/açılma, açık yeni yaşamla onarım, 3 dış saldırı konumu. NavMesh algoritması veya zombi türü bilmez. |
| `ProximityRepair` (Train) | Oyuncu canlı mı/yakın mı, süre ve iptal; HUD olmadan da çalışır. |
| `EntryPortal` / `AgentMotor` (Navigation) | Carving/collider ve güvenli eşik kapanışı; yalnızca agent hareketini sürme. Yeni brain üzerinde eski `PortalNavigator` yoktur. |
| `DoorPanels` (Train) | Portal açık/kapalı durumuna göre alt fizik/atış panelini ve üst cam görselini açar/kapatır. Kapı canı veya oyuncu izni kararı vermez. |
| `EnemyBrain` / `MeleeAttack` (Enemies) | 0.15 s aralıklı karar, kapı → giriş → oyuncu hedefi; saldırı hazırlığı ve isabet anında yaşam/menzil/görüş kontrolü. |
| `EnemyPool` (Enemies) | 12 nesneyle ön ısınma, sınır dolunca üretimi reddetme, nav doğuş doğrulaması ve açık yaşam sıfırlama. Ölüm iadesi Health bildirim zinciri bittikten sonra LateUpdate'tadır. |
| `TargetRegistry` / `HitscanPistol` (Combat.Unity) | Açık canlı hedef kaydı, atış/şarjör/doldurma ve gerçek isabet; ses/efekt hasar üretmez. |
| `MoveInput` / `PlayerMotor` / `AutoAim` (Player) | Girdi → CharacterController hareketi; kayıtlı adaylardan görüşü açık en yakını seçme. Train/Enemies assembly'sine bağlı değildir. |
| `MovementArea` (Player) | İsteğe bağlı, dik yerel XZ hareket sınırı; gövde yarıçapı/skin payıyla normal hareketi içeride tutar. PlayerMotor alanı değiştirebilir/kaldırabilir; Train/NavMesh bağımlılığı yoktur. |
| `GameplaySandboxController` / kurulum aracı | Geçici HUD, tracer, Restart ve kabul fixture'ı. Runtime modülleri Sandbox'a bağlı değildir. |

Kapı canı sıfırlanınca saldırı konumları bırakılır ve portal açılır. Onarım canı doldurur ama eşik doluysa portal `ClosePending` durumunda kalır; fiziksel engel gövdelerin içine kurulmaz. Eşikteki zombi yolunu tamamlar, yeni dış giriş hedefi kabul edilmez. İçerideki zombi onarılan dış kapıya geri dönmez; oyuncuya saldırır. Kalkışın giriş izni/üyelik politikası M4'te ayrıca bağlanacak.

Zombilerin içerideki amacı eski labdaki sabit grid noktasına tam olarak dizilmek değildir; oyuncunun saldırı menziline girmektir. Kapı önündeki 3 saldırı konumu ölüm/havuza dönüş/kırılmada bırakılır; boşalan konumu bekleyen zombi alabilir. Bu ilk yaklaşım 30–100 düşmanlı üretim crowd çözümünün doğrulandığı anlamına gelmez. Dar alan/çok kapı/çeşitli boylar ve büyük kalabalık M6'da ayrıca ölçülür.

Pool dönüşünde hedef, portal, slot, wind-up, zamanlayıcı ve agent path temizlenir; target registry'den çıkarılır ve OnDisable ölüm aboneliğini bırakır. Yeni yaşam sürümü eski vuruşu reddeder. Geçersiz nav doğuşu poolu büyütmez/azaltmaz. Despawn/restart bir öldürme ödülü değildir; cüzdan/ekonomi M7'de bağlanacak. Hasar türü/ödül sahibi M2 sözleşmesinden taşınır.

## Doğrulama ve açık sınırlar

2026-10-07 Editor Play ilk gözlem: 3 zombi sağlam kapıyı kırdı, içeri girdi, oyuncu canı 100 → 88 oldu; otomatik tabanca zombileri öldürdü, aktif sayı 0 ve oluşturulan pool sayısı 12 kaldı. 15 isabet iki şarjör kullandı; kalan mermi 1/8 oldu. Yeni runtime exception görülmedi.

`Check 10 door / repair / pool cycles` geçici fixture'ı otomatik ateşi/girdiyi kapatır; gerçek enemy/repair kodunu kullanır. Test konumlarına taşıma yalnızca fixture işlevidir, normal yürüyüşte teleport yapılmaz. Kontrol: kapıya hasar → kırılma → giriş → oyuncuya hasar; kısa onarım → ayrılınca sıfır ilerleme; tam onarım → güvenli kapanış; ilk döngüde gerçek CharacterController eşikte tutulur; zombi ölümü tek iade, aynı nesnenin yeni yaşamına eski vuruş reddi ve geçersiz doğuş reddi. Sonuç `Logs/GameplaySandboxReport.json` ve Console'a yazılır.

2026-10-07 **10/10 PASS**, WindowsEditor, rapor UTC `2026-10-07T17:46:27.2328192Z`: yukarıdaki birleşik kapı/onarım/havuz kontrolü. Testte ilk döngüde eşikte oyuncu gövdesi vardır; normal geçişte teleport kullanılmaz. Kalabalık, farklı agent boyları ve istasyon ayrılması bu sonucun kapsamı değildir.

Cam kapı revizyonundan önceki havuz doğuş düzeltmesi sonrası **10/10 PASS**, UTC `2026-10-07T17:53:32.5319241Z`. Collider yeni konumda etkinleştirilir; önceki yaşam konumu fizik sorgularına taşınmaz. O sürümde sağlam kapı atışı engelliyordu. Kullanıcı sonraki değerlendirmede bu kuralı D15 ile değiştirdi; eski 10/10 raporu yeni cam davranışının kanıtı değildir. Aynı 10 döngü düğmesinin görüş beklentisi artık camdan görülebilir hedefi doğrular.

## D15/D16 — camdan savunma ve oyuncunun vagon sınırı

Gameplay sahnesindeki tam yüksekliğe sahip fizik/geçiş kapısı `ShotTransparent` katmanındadır. **Yalnızca silahın Inspector `hitMask` ayarı** bu katmanı dışlar; CharacterController çarpışması, melee sorguları, portal işgal kontrolü ve NavMesh carving bu fiziksel engeli kullanmaya devam eder. Alt 0.7 m dolu panel normal katmanda ayrı collider ile mermiyi durdurur; üst cam görseldir. Kırılma/güvenli kapanış iki paneli portal durumuyla senkronlar. Global layer collision matrisi değiştirilmez. Yeni silah/turret aynı maskeyi kullanmalı; tüm duvarları veya tüm kapı mantığını atıştan hariç tutmamalı.

`Check window shots / walls / player boundary` fixture'ı güncel kuralları sınar: sağlam camdan gerçek atış dış zombinin canını azaltır, kapı canı değişmez; duvar hedef seçimini ve gerçek isabeti engeller; aşağı yönlü atış dolu alt panelde durur; büyük hareket adımı oyuncuyu sağlam/kırık/onarılmış kapıdan dışarı çıkarmaz; gerçek zombi kapıyı hasarla kırıp yürüyerek içeri girer; yakınlık onarımı kapatır ve onarılmış camdan atış yeniden geçer; hareket alanı açıkça kaldırıldığında istasyona hareket mümkündür. Test yerleşimleri fixture içindir. `Logs/GameplayDefenseReport.json` sonuç/UTC/kapsamı saklar. Eski prototip ve nav/health laboratuvarları değiştirilmedi.

Güncel sürüm **PASS**, WindowsEditor, UTC `2026-10-07T18:53:55.9235675Z`: bu odaklı birleşik kontrolün tamamı geçti. Bu turda 10 döngü yeniden çalıştırılmadı; önceki 10/10 ve yeni D15/D16 kanıtları ayrı kapsamdır. Son test kodu Play açıkken yenilendiğinde geçici runtime alanları kayboldu ve HUD NullReference üretti; Play durdurulup temiz çalıştırmada kontrol tekrar yapıldı. Kaynak kod değişiklikleri laboratuvarda Play kapalıyken yapılmalı; canlı C# reload kabul testi değildir. Android yeniden denenmedi.

**Henüz tamamlandı sayılmayanlar:** sol dokunmatik kontrolün cihaz hissi ve eğlence değerlendirmesi kullanıcı denemesi gerektirir. Bu turda Android build yapılmadı. Karakter animasyonu, satın alınmış silah VFX'leri, nihai HUD ve denge cilası bu teknik parçada tamamlandı sayılmaz. M3 kullanıcı değerlendirmesi bitmeden M4 kapsamı açılmaz.

2026-10-07 kullanıcı oynanışı izleyip iyi çalıştığını belirtti ve sonraki aşamaya geçilmesini istedi. M3 PC değerlendirmesi kabul edildi; dokunmatik cihaz hissi uygun zamanda ayrıca kontrol edilecek.
