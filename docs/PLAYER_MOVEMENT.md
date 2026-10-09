# Oyuncu ve bot hareket sözleşmesi

2026-10-09 kullanıcı düzeltmesi: kalabalık açık kapıda oyuncuyu dışarı itebiliyordu; hedef yokken WASD dönüşü anlıktı. İnsan ve bot aynı `PlayerMotor` düzeltmesini kullanır.

`MovementArea` isteğe bağlı, vagonun yerel X/Z iç sınırıdır. Motor yalnız istenen hareketi değil, `CharacterController.Move` çarpışma çözümünden çıkan **gerçek kapsül merkezini** de bu alana sınırlar. Yarıçapa `skinWidth` ve dünya ölçeği katılır. `LateUpdate` son konumu tekrar korur; kayıt kare sonunda geçerli konum yakalar. Düzeltme controller'ı kapatıp açmaz; sahibin turret ile çarpışmama eşleşmeleri korunur. Yeni collider, fizik taraması, NavMesh üretimi veya düşman başına ek döngü yoktur.

Bu oyuncu/bot sınırı açık kapıda da geçerlidir. Zombilerin kapıdan girişini, camdan atışı veya kapı hasar/onarımını kapatmaz. Gelecekte istasyonda yürüme için hareket alanı değiştirilebilir ya da `SetMovementArea(null)` ile kaldırılabilir; kalıcı tren bağımlılığı eklenmez. Sanat modeli collider veya root motion ile mekanik kökü ayrıca sürmez.

M8s Solo: `MovementArea.ConfigureRegions` bağlı açık odalar ve bindirmeli ara koridor dikdörtgenleri alır. Her aday gerçek kapsül yarıçapıyla içeri alınır; en yakın geçerli aday seçilir. Kapalı kapının ötesindeki açık fakat ayrık oda oyuncunun bileşenine eklenmez. Büyük tek tren dikdörtgeni kullanılmadığı için vagon arası açık boşluğa veya dış kapıdan platforma kaçılmaz. Hacim dizisi sadece kilit/kapı/güncel vagon değişince kurulur, her kare tahsis edilmez. Battle ve eski laboratuvarlar tek vagon sınırını kullanır; fiziksel Solo eşiği respawn/koruma üretmez.

Hedef yokken motor, verilen yatay yürüyüş yönüne **720 derece/sn** hızla döner (`walkingTurnSpeed`, Inspector). 90° yaklaşık 0,125 sn, 180° yaklaşık 0,25 sn sürer. WASD, dokunmatik joystick ve bot dünya yönü aynı dönüş kuralını kullanır; joystick tek başına önceki anlık dönüşü çözmüyordu. Girdi yokken son yön korunur; kalabalığın fiziksel itmesi kendi başına yüz yönü seçmez. Geçerli görünür ateş hedefi yürüyüşün önüne geçer ve önceki hızlı nişan dönüşünü korur. Duraklamada dönüş ilerlemez.

## PC kontrolü

`TrainIntegration` Play'de **F3**: iki karşı platformdaki açık kapıda sekiz fiziksel zombi kapsülüyle sıkıştırma; her kapıda 90 kare boyunca hareket sonrası ve kare sonunda tam kapsül sınırı; normal zombi girişinin devam etmesi; 90° dönüşün anlık olmaması, 90°/180° tamamlanması ve boşta yönün korunması. Test ayrı bir hareket uygulaması kullanmaz. İlk kontrol eski kodda `Collision pushed player outside open door: Door 1 south` ile başarısız oldu; düzeltmeden sonra aynı senaryo geçti.

**F4** görünür hedefin yürüyüş yönünü geçersiz kılmasını ve kayıt/devam uyumunu kontrol eder; yürüyüş kontrolü artık yumuşak dönüşün tamamlanmasını bekler. Bu değişiklikte gerçek telefon dokunma hissi yeniden değerlendirilmedi; yürüyüş dönüş hızı oynanış geri bildirimiyle ayarlanabilir. Kanıt [generated/player-movement-pc-acceptance.txt](generated/player-movement-pc-acceptance.txt).
